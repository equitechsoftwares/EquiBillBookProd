using EquiBillBook.Controllers.WebApi.Common;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Vonage.Common.Monads;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    //[IdentityBasicAuthenticationAttribute]
    public class TaxController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        GstController oGstController = new GstController();

        #region Tax
        public async Task<IHttpActionResult> AllTaxs(ClsTaxVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            //&& a.IsTaxGroup == false
            && a.CanDelete == true && a.IsCompositionScheme == false
            ).ToList().Select(a => new
            {
                a.IsPredefined,
                //a.CanDelete,
                TaxId = a.TaxId,
                a.TaxPercent,
                Tax = a.Tax,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                ForTaxGroupOnly = a.ForTaxGroupOnly,
                IsTaxGroup = a.IsTaxGroup,
                SubTaxIds = a.IsTaxGroup == false ? "" : string.Join(",", oConnectionContext.DbClsTaxMap.Where(b => b.CompanyId == obj.CompanyId
                && b.TaxId == a.TaxId && b.IsActive == true && b.IsDeleted == false).Select(b => oConnectionContext.DbClsTax.Where(c => c.TaxId == b.SubTaxId).Select(c => c.Tax).FirstOrDefault()).ToList()),
                a.TaxTypeId,
                TaxType = oConnectionContext.DbClsTaxType.Where(b => b.TaxTypeId == a.TaxTypeId).Select(b => b.TaxType).FirstOrDefault(),
            }).ToList();

            // var det1 = oConnectionContext.DbClsTax
            //.Where(a => a.CompanyId == obj.CompanyId
            //        && a.IsDeleted == false && a.IsCancelled == false
            //        && a.CanDelete == true
            //        && a.IsCompositionScheme == false)
            //.Select(a => new
            //{
            //    a.IsPredefined,
            //    a.TaxId,
            //    a.TaxPercent,
            //    Tax = a.Tax,
            //    IsActive = a.IsActive,
            //    IsDeleted = a.IsDeleted,
            //    AddedBy = a.AddedBy,
            //    AddedOn = a.AddedOn,
            //    ModifiedBy = a.ModifiedBy,
            //    ModifiedOn = a.ModifiedOn,
            //    CompanyId = a.CompanyId,
            //    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
            //    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            //    ForTaxGroupOnly = a.ForTaxGroupOnly,
            //    IsTaxGroup = a.IsTaxGroup,
            //    SubTaxIdsList = oConnectionContext.DbClsTaxMap
            //                   .Where(b => b.CompanyId == obj.CompanyId
            //                          && b.TaxId == a.TaxId
            //                          && b.IsActive == true
            //                          && b.IsDeleted == false && b.IsCancelled == false)
            //                   .Select(b => oConnectionContext.DbClsTax.Where(c => c.TaxId == b.SubTaxId).Select(c => c.Tax).FirstOrDefault())
            //                   .ToList(),
            //    a.TaxTypeId,
            //    TaxType = oConnectionContext.DbClsTaxType.Where(b => b.TaxTypeId == a.TaxTypeId).Select(b => b.TaxType).FirstOrDefault()
            //}).ToList();

            // // After getting the data, apply string.Join in memory for SubTaxIds
            // var det = det1.Select(a => new
            // {
            //     a.IsPredefined,
            //     a.TaxId,
            //     a.TaxPercent,
            //     Tax = a.Tax,
            //     IsActive = a.IsActive,
            //     IsDeleted = a.IsDeleted,
            //     AddedBy = a.AddedBy,
            //     AddedOn = a.AddedOn,
            //     ModifiedBy = a.ModifiedBy,
            //     ModifiedOn = a.ModifiedOn,
            //     CompanyId = a.CompanyId,
            //     AddedByCode = a.AddedByCode,
            //     ModifiedByCode = a.ModifiedByCode,
            //     ForTaxGroupOnly = a.ForTaxGroupOnly,
            //     IsTaxGroup = a.IsTaxGroup,
            //     SubTaxIds = a.IsTaxGroup ? string.Join(",", a.SubTaxIdsList) : "",
            //     a.TaxTypeId,
            //     TaxType = a.TaxType
            // }).ToList();

            if (obj.TaxTypeId != 0)
            {
                det = det.Where(a => a.TaxTypeId == obj.TaxTypeId).ToList();
            }

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Tax.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Taxs = det.OrderByDescending(a => a.TaxId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Tax(ClsTax obj)
        {
            var det = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.IsPredefined,
                //a.CanDelete,
                TaxId = a.TaxId,
                a.TaxPercent,
                Tax = a.Tax,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                ForTaxGroupOnly = a.ForTaxGroupOnly,
                a.PurchaseAccountId,
                a.SalesAccountId,
                a.SupplierPaymentAccountId,
                a.CustomerPaymentAccountId,
                a.ExpenseAccountId,
                a.IncomeAccountId,
                a.TaxTypeId
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Tax = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTax(ClsTaxVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Tax == null || obj.Tax == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTax" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Fields marked with * is mandatory",
                    //    Data = new
                    //    {
                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                //if (obj.TaxPercent == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxPercent" });
                //    isError = true;
                //}

                if (oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == obj.Tax.ToLower() && a.TaxPercent == obj.TaxPercent && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Tax Name exists", Id = "divTax" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Duplicate Tax Name exists",
                    //    Data = new
                    //    {
                    //    }
                    //}; return await Task.FromResult(Ok(data));
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

                ClsTax oTax = new ClsTax()
                {
                    IsPredefined = false,
                    CanDelete = true,
                    Tax = obj.Tax,
                    TaxPercent = obj.TaxPercent,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsTaxGroup = false,
                    ForTaxGroupOnly = obj.ForTaxGroupOnly,
                    PurchaseAccountId = obj.PurchaseAccountId,
                    SalesAccountId = obj.SalesAccountId,
                    SupplierPaymentAccountId = obj.SupplierPaymentAccountId,
                    CustomerPaymentAccountId = obj.CustomerPaymentAccountId,
                    ExpenseAccountId = obj.ExpenseAccountId,
                    IncomeAccountId = obj.IncomeAccountId,
                    TaxTypeId = obj.TaxTypeId,
                };
                oConnectionContext.DbClsTax.Add(oTax);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax List",
                    CompanyId = obj.CompanyId,
                    Description = "Tax List \"" + obj.Tax + "\" created",
                    Id = oTax.TaxId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax created successfully",
                    Data = new
                    {
                        Tax = oTax
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateTax(ClsTaxVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Tax == null || obj.Tax == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTax" });
                    isError = true;
                }

                //if (obj.TaxPercent == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxPercent" });
                //    isError = true;
                //}

                if (oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == obj.Tax.ToLower() && a.TaxPercent == obj.TaxPercent && a.TaxId != obj.TaxId && a.CompanyId == obj.CompanyId
                && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Tax Name exists", Id = "divTax" });
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

                ClsTax oTax = new ClsTax()
                {
                    TaxId = obj.TaxId,
                    TaxPercent = obj.TaxPercent,
                    Tax = obj.Tax,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    ForTaxGroupOnly = obj.ForTaxGroupOnly,
                    PurchaseAccountId = obj.PurchaseAccountId,
                    SalesAccountId = obj.SalesAccountId,
                    SupplierPaymentAccountId = obj.SupplierPaymentAccountId,
                    CustomerPaymentAccountId = obj.CustomerPaymentAccountId,
                    ExpenseAccountId = obj.ExpenseAccountId,
                    IncomeAccountId = obj.IncomeAccountId,
                    TaxTypeId = obj.TaxTypeId
                };
                oConnectionContext.DbClsTax.Attach(oTax);
                oConnectionContext.Entry(oTax).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.Tax).IsModified = true;
                //oConnectionContext.Entry(oTax).Property(x => x.TaxPercent).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.ModifiedOn).IsModified = true;
                //oConnectionContext.Entry(oTax).Property(x => x.ForTaxGroupOnly).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.PurchaseAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.SalesAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.SupplierPaymentAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.CustomerPaymentAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.ExpenseAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.IncomeAccountId).IsModified = true;
                //oConnectionContext.Entry(oTax).Property(x => x.TaxTypeId).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax List",
                    CompanyId = obj.CompanyId,
                    Description = "Tax List \"" + obj.Tax + "\" updated",
                    Id = oTax.TaxId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveTaxs(ClsTaxVm obj)
        {
            var det = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
            a.IsActive == true && a.IsTaxGroup == false
            //&& a.ForTaxGroupOnly == false
            && a.IsCompositionScheme == false
            ).Select(a => new
            {
                a.CanDelete,
                TaxId = a.TaxId,
                a.TaxPercent,
                Tax = a.Tax,
            }).OrderBy(a => a.Tax).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Taxs = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }
        #endregion

        #region TaxGroup
        public async Task<IHttpActionResult> AllTaxGroups(ClsTaxVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
            a.IsTaxGroup == true).AsEnumerable().Select(a => new
            {
                TaxId = a.TaxId,
                a.TaxPercent,
                Tax = a.Tax,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                SubTaxIds = string.Join(",", oConnectionContext.DbClsTaxMap.Where(b => b.CompanyId == obj.CompanyId
                && b.TaxId == a.TaxId && b.IsActive == true && b.IsDeleted == false).Select(b => oConnectionContext.DbClsTax.Where(c => c.TaxId == b.SubTaxId).Select(c => c.Tax).FirstOrDefault()).ToList()),
                //SubTaxIds = string.Join(",", a.SubTaxIds.Split(',').Select(b => oConnectionContext.DbClsTax.Where(c => c.CompanyId == obj.CompanyId).AsEnumerable().Where(c => c.TaxId == Convert.ToInt64(b)).Select(c => c.Tax).FirstOrDefault()).ToList()),
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();
            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SubTaxIds.ToLower().Contains(obj.Search.ToLower()) || a.Tax.ToLower().Contains(obj.Search.ToLower())).ToList();
            }
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Taxs = det.OrderByDescending(a => a.TaxId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxGroup(ClsTax obj)
        {
            var det = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId && a.CompanyId == obj.CompanyId).AsEnumerable().Select(a => new
            {
                TaxId = a.TaxId,
                a.TaxPercent,
                Tax = a.Tax,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                a.IsTaxGroup,
                SubTaxs = oConnectionContext.DbClsTaxMap.Where(b => b.CompanyId == a.CompanyId && b.IsActive == true &&
                b.IsDeleted == false && b.TaxId == a.TaxId).Select(b => b.SubTaxId),
                a.PurchaseAccountId,
                a.SalesAccountId,
                a.SupplierPaymentAccountId,
                a.CustomerPaymentAccountId,
                a.ExpenseAccountId,
                a.IncomeAccountId
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Tax = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertTaxGroup(ClsTaxVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Tax == null || obj.Tax == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxGroup" });
                    isError = true;
                }

                if (obj.SubTaxs[0] == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubTax" });
                    isError = true;
                }

                if (oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == obj.Tax.ToLower() && a.IsTaxGroup == true && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Tax Group Name exists", Id = "divTaxGroup" });
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

                obj.SubTaxIds = "";
                obj.TaxPercent = 0;

                if (obj.SubTaxs != null)
                {
                    foreach (var item in obj.SubTaxs)
                    {
                        //obj.SubTaxIds = obj.SubTaxIds + "," + item;

                        long TaxId = Convert.ToInt64(item);
                        decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.TaxId == TaxId).Select(a => a.TaxPercent).FirstOrDefault();
                        obj.TaxPercent = obj.TaxPercent + TaxPercent;
                    }
                }

                ClsTax oTax = new ClsTax()
                {
                    Tax = obj.Tax,
                    TaxPercent = obj.TaxPercent,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsTaxGroup = true,
                    //SubTaxIds = obj.SubTaxIds.TrimStart(',')
                    PurchaseAccountId = obj.PurchaseAccountId,
                    SalesAccountId = obj.SalesAccountId,
                    SupplierPaymentAccountId = obj.SupplierPaymentAccountId,
                    CustomerPaymentAccountId = obj.CustomerPaymentAccountId,
                    ExpenseAccountId = obj.ExpenseAccountId,
                    IncomeAccountId = obj.IncomeAccountId,
                    CanDelete = true
                };
                oConnectionContext.DbClsTax.Add(oTax);
                oConnectionContext.SaveChanges();

                foreach (var item in obj.SubTaxs)
                {
                    ClsTaxMap oClsTaxMap = new ClsTaxMap()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        TaxId = oTax.TaxId,
                        SubTaxId = Convert.ToInt64(item)
                    };
                    oConnectionContext.DbClsTaxMap.Add(oClsTaxMap);
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Group",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Group \"" + obj.Tax + "\" created",
                    Id = oTax.TaxId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Group created successfully",
                    Data = new
                    {
                        Tax = oTax
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateTaxGroup(ClsTaxVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Tax == null || obj.Tax == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxGroup" });
                    isError = true;
                }

                if (obj.SubTaxs[0] == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divSubTax" });
                    isError = true;
                }

                if (oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == obj.Tax.ToLower() && a.IsTaxGroup == true && a.TaxId != obj.TaxId && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Tax Group Name exists", Id = "divTaxGroup" });
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

                obj.SubTaxIds = "";
                obj.TaxPercent = 0;

                if (obj.SubTaxs != null)
                {
                    foreach (var item in obj.SubTaxs)
                    {
                        //obj.SubTaxIds = obj.SubTaxIds + "," + item;

                        long TaxId = Convert.ToInt64(item);
                        decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.TaxId == TaxId).Select(a => a.TaxPercent).FirstOrDefault();
                        obj.TaxPercent = obj.TaxPercent + TaxPercent;
                    }
                }

                ClsTax oTax = new ClsTax()
                {
                    TaxId = obj.TaxId,
                    TaxPercent = obj.TaxPercent,
                    //SubTaxIds = obj.SubTaxIds.TrimStart(','),
                    Tax = obj.Tax,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    PurchaseAccountId = obj.PurchaseAccountId,
                    SalesAccountId = obj.SalesAccountId,
                    SupplierPaymentAccountId = obj.SupplierPaymentAccountId,
                    CustomerPaymentAccountId = obj.CustomerPaymentAccountId,
                    ExpenseAccountId = obj.ExpenseAccountId,
                    IncomeAccountId = obj.IncomeAccountId
                };
                oConnectionContext.DbClsTax.Attach(oTax);
                oConnectionContext.Entry(oTax).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.Tax).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.TaxPercent).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.ModifiedOn).IsModified = true;
                //oConnectionContext.Entry(oTax).Property(x => x.SubTaxIds).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.PurchaseAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.SalesAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.SupplierPaymentAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.CustomerPaymentAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.ExpenseAccountId).IsModified = true;
                oConnectionContext.Entry(oTax).Property(x => x.IncomeAccountId).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "update \"tblTaxMap\" set \"IsActive\"= False where \"TaxId\"=" + oTax.TaxId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                foreach (var item in obj.SubTaxs)
                {
                    long SubTaxId = Convert.ToInt64(item);

                    long TaxMapId = oConnectionContext.DbClsTaxMap.Where(a => a.TaxId == oTax.TaxId &&
                    a.SubTaxId == SubTaxId && a.IsDeleted == false).Select(a => a.TaxMapId).FirstOrDefault();
                    if (TaxMapId == 0)
                    {
                        ClsTaxMap oClsTaxMap = new ClsTaxMap()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            TaxId = oTax.TaxId,
                            SubTaxId = Convert.ToInt64(item)
                        };
                        oConnectionContext.DbClsTaxMap.Add(oClsTaxMap);
                        oConnectionContext.SaveChanges();
                    }
                    else
                    {
                        ClsTaxMap oClsTaxMap = new ClsTaxMap()
                        {
                            TaxMapId = TaxMapId,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            IsActive = true,
                            IsDeleted = false,
                            TaxId = oTax.TaxId,
                            SubTaxId = Convert.ToInt64(item)
                        };
                        oConnectionContext.DbClsTaxMap.Attach(oClsTaxMap);
                        oConnectionContext.Entry(oClsTaxMap).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsTaxMap).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsTaxMap).Property(x => x.IsActive).IsModified = true;
                        oConnectionContext.Entry(oClsTaxMap).Property(x => x.IsDeleted).IsModified = true;
                        oConnectionContext.Entry(oClsTaxMap).Property(x => x.TaxId).IsModified = true;
                        oConnectionContext.Entry(oClsTaxMap).Property(x => x.SubTaxId).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Tax Group",
                    CompanyId = obj.CompanyId,
                    Description = "Tax Group \"" + obj.Tax + "\" updated",
                    Id = oTax.TaxId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Tax Group updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        #endregion

        public async Task<IHttpActionResult> TaxActiveInactive(ClsTaxVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var Type = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.IsTaxGroup == false ? "Tax" : "Tax Group").FirstOrDefault();
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsTax oClsRole = new ClsTax()
                {
                    TaxId = obj.TaxId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTax.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = Type == "Tax" ? "Tax List" : "Tax Group",
                    CompanyId = obj.CompanyId,
                    Description = (Type == "Tax" ? "Tax List" : "Tax Group") + " \"" + oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.Tax).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.TaxId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = Type + (obj.IsActive == true ? " activated" : " deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxDelete(ClsTaxVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int UserCount = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.TaxId == obj.TaxId).Count();

                int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.TaxId == obj.TaxId).Count();

                int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.TaxId == obj.TaxId).Count();

                int PurchaseQuotationCount = (from a in oConnectionContext.DbClsPurchaseQuotation
                                              join b in oConnectionContext.DbClsPurchaseQuotationDetails
                                           on a.PurchaseQuotationId equals b.PurchaseQuotationId
                                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.TaxId == obj.TaxId
                                              select a.PurchaseQuotationId).Count();

                int PurchaseOrderCount = (from a in oConnectionContext.DbClsPurchaseOrder
                                          join b in oConnectionContext.DbClsPurchaseOrderDetails
                                           on a.PurchaseOrderId equals b.PurchaseOrderId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.TaxId == obj.TaxId
                                          select a.PurchaseOrderId).Count();

                int PurchaseCount = (from a in oConnectionContext.DbClsPurchase
                                     join b in oConnectionContext.DbClsPurchaseDetails
                                   on a.PurchaseId equals b.PurchaseId
                                     where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                   && b.TaxId == obj.TaxId
                                     select a.PurchaseId).Count();

                int SalesQuotationCount = (from a in oConnectionContext.DbClsSalesQuotation
                                           join b in oConnectionContext.DbClsSalesQuotationDetails
                                           on a.SalesQuotationId equals b.SalesQuotationId
                                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.TaxId == obj.TaxId
                                           select a.SalesQuotationId).Count();

                int SalesOrderCount = (from a in oConnectionContext.DbClsSalesOrder
                                       join b in oConnectionContext.DbClsSalesOrderDetails
                                           on a.SalesOrderId equals b.SalesOrderId
                                       where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.TaxId == obj.TaxId
                                       select a.SalesOrderId).Count();

                int SalesProformaCount = (from a in oConnectionContext.DbClsSalesProforma
                                          join b in oConnectionContext.DbClsSalesProformaDetails
                                           on a.SalesProformaId equals b.SalesProformaId
                                          where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                           && b.TaxId == obj.TaxId
                                          select a.SalesProformaId).Count();

                int DeliveryChallanCount = (from a in oConnectionContext.DbClsDeliveryChallan
                                            join b in oConnectionContext.DbClsDeliveryChallanDetails
                                           on a.DeliveryChallanId equals b.DeliveryChallanId
                                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                           && b.TaxId == obj.TaxId
                                            select a.DeliveryChallanId).Count();

                int SalesCount = (from a in oConnectionContext.DbClsSales
                                  join b in oConnectionContext.DbClsSalesDetails
                                   on a.SalesId equals b.SalesId
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                   && b.TaxId == obj.TaxId
                                  select a.SalesId).Count();

                if (UserCount > 0 || BranchCount > 0 || ItemCount > 0 || PurchaseQuotationCount > 0
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
                var Type = oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.IsTaxGroup == false ? "Tax" : "Tax Group").FirstOrDefault();

                ClsTax oClsRole = new ClsTax()
                {
                    TaxId = obj.TaxId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsTax.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = Type == "Tax" ? "Tax List" : "Tax Group",
                    CompanyId = obj.CompanyId,
                    Description = (Type == "Tax" ? "Tax List" : "Tax Group") + " \"" + oConnectionContext.DbClsTax.Where(a => a.TaxId == obj.TaxId).Select(a => a.Tax).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.TaxId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = Type + " deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchTaxPercent(ClsTaxVm obj)
        {
            decimal Tax = 0;
            if (obj.SubTaxs != null)
            {
                foreach (var item in obj.SubTaxs)
                {
                    long TaxId = Convert.ToInt64(item);
                    decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.TaxId == TaxId).Select(a => a.TaxPercent).FirstOrDefault();
                    Tax = Tax + TaxPercent;
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxPercent = Tax
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAllTaxs(ClsTaxVm obj)
        {
            var det = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
             && a.ForTaxGroupOnly == false).Select(a => new
             {
                 TaxId = a.TaxId,
                 a.TaxPercent,
                 Tax = a.Tax,
                 a.CanDelete,
                 a.TaxTypeId,
                 a.IsCompositionScheme
             }).OrderBy(a => a.TaxId).ToList();

            if (obj.IsCompositionScheme == false)
            {
                det = det.Where(a => a.IsCompositionScheme == false).ToList();
            }
            else
            {
                det = det.Where(a => a.IsCompositionScheme == true).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Taxs = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxBreakups(ClsSalesVm obj)
        {
            // Handle null or empty Taxs collection
            if (obj == null || obj.Taxs == null || !obj.Taxs.Any())
            {
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        Taxs = new List<object>(),
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            var AllTaxs = obj.Taxs.Select(a => new
            {
                IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                a.TaxId,
                a.AmountExcTax
            }).ToList();

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
                    Taxs = finalTaxs,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        //        public async Task<IHttpActionResult> TaxReport(ClsTaxVm obj)
        //        {
        //            //var userDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).AsEnumerable().Select(a => new
        //            //{
        //            //    a.IsCompany,
        //            //    a.UserRoleId,
        //            //    BranchIds = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
        //            //  && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, b.Branch }).ToList() :
        //            //   oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
        //            //     && b.IsDeleted == false && b.IsCancelled == false).Select(b => new { b.BranchId, Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() }).ToList(),
        //            //}).FirstOrDefault();

        //            //if (obj.BranchId == 0)
        //            //{
        //            //    obj.BranchId = userDetails.BranchIds.Count == 0 ? 0 : userDetails.BranchIds[0].BranchId;
        //            //}

        //            if (obj.FromDate == DateTime.MinValue)
        //            {
        //                int d = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.FinancialYearStartMonth).FirstOrDefault();

        //                obj.FromDate = Convert.ToDateTime("01-" + Convert.ToString(d) + "-" + Convert.ToString(DateTime.Now.Year));
        //                if (obj.FromDate > DateTime.Now)
        //                {
        //                    obj.FromDate = obj.FromDate.AddYears(-1);
        //                }

        //                obj.ToDate = obj.FromDate.AddMonths(11);

        //                int days = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(obj.ToDate.Year, obj.ToDate.Month);

        //                obj.ToDate = obj.ToDate.AddDays(days - 1);
        //            }

        //            obj.FromDate = obj.FromDate.AddHours(5).AddMinutes(30);
        //            obj.ToDate = obj.ToDate.AddHours(5).AddMinutes(30);

        //            if (obj.PageSize == 0)
        //            {
        //                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
        //            }

        //            int skip = obj.PageSize * (obj.PageIndex - 1);
        //            int PurchasePageCount = 0, SalePageCount = 0, ExpensePageCount = 0, IncomePageCount = 0;

        //            List<ClsPurchaseVm> purchaseTax1;
        //            List<ClsPurchaseVm> purchaseTax2;
        //            List<ClsPurchaseVm> purchaseTax3 = new List<ClsPurchaseVm>();
        //            if (obj.BranchId == 0)
        //            {
        //                purchaseTax1 = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //            //&& a.BranchId == obj.BranchId 
        //            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //             && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
        //                DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate).Select(a => new ClsPurchaseVm
        //                {
        //                    TaxId = a.TaxId,
        //                    TaxAmount = a.TaxAmount,
        //                    PurchaseId = a.PurchaseId,
        //                    PurchaseDate = a.PurchaseDate,
        //                    ReferenceNo = a.ReferenceNo,
        //                    SupplierId = a.SupplierId,
        //                    SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
        //                    SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
        //                    TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.TaxNo).FirstOrDefault(),
        //                    Subtotal = a.Subtotal,
        //                    PaymentType = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false
        //                    && b.PurchaseId == a.PurchaseId).Select(b => b.AccountId).Count() == 0 ? "" :
        //                 oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false
        //                 && b.PurchaseId == a.PurchaseId).Select(b => b.AccountId).Count() == 1 ?
        //                 oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false
        //                 && b.PurchaseId == a.PurchaseId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //                 ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //                    TotalDiscount = a.TotalDiscount,
        //                    Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                    { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                              join c in oConnectionContext.DbClsTaxMap
        //                                                                                                      on b.TaxId equals c.TaxId
        //                                                                                              where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                              select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                           where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                           select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).ToList()
        //                }).ToList();

        //                purchaseTax2 = (from x in oConnectionContext.DbClsPurchaseDetails
        //                                join a in
        //                     oConnectionContext.DbClsPurchase on x.PurchaseId equals a.PurchaseId
        //                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && x.IsDeleted == false && x.IsCancelled == false
        //                        //&& a.BranchId == obj.BranchId 
        //                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //                         && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
        //                            DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
        //                                select new ClsPurchaseVm
        //                                {
        //                                    TaxId = x.TaxId,
        //                                    TaxAmount = x.TaxAmount,
        //                                    PurchaseId = x.PurchaseId,
        //                                    PurchaseDate = a.PurchaseDate,
        //                                    ReferenceNo = a.ReferenceNo,
        //                                    SupplierId = a.SupplierId,
        //                                    SupplierName = "",
        //                                    SupplierMobileNo = "",
        //                                    TaxNo = "",
        //                                    Subtotal = x.PurchaseExcTax,
        //                                    PaymentType = "",
        //                                    TotalDiscount = a.TotalDiscount,
        //                                    Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == x.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                                    { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PurchaseExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                 join c in oConnectionContext.DbClsTaxMap
        //                                                                                                                                 on b.TaxId equals c.TaxId
        //                                                                                                                                 where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                                 select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * x.Quantity * x.PurchaseExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                                                                                 where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                                                                                 select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PurchaseExcTax }).ToList()
        //                                }).ToList();

        //                purchaseTax3 = purchaseTax1;

        //                foreach (var item in purchaseTax2)
        //                {
        //                    foreach (var tom in purchaseTax3.Where(w => w.PurchaseId == item.PurchaseId))
        //                    {
        //                        foreach (var inner in item.Taxs)
        //                        {
        //                            if (tom.Taxs.Where(w => w.TaxId == item.TaxId).Count() == 0)
        //                            {
        //                                tom.Taxs.Add(new ClsTaxVm { TaxId = inner.TaxId, TaxAmount = inner.TaxAmount });
        //                            }
        //                            else
        //                            {
        //                                decimal prevAmount = tom.Taxs.Where(a => a.TaxId == item.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                                foreach (var i in tom.Taxs.Where(w => w.TaxId == item.TaxId))
        //                                {
        //                                    i.TaxAmount = prevAmount + inner.TaxAmount;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                purchaseTax1 = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //            && a.BranchId == obj.BranchId
        //             && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
        //                DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate).Select(a => new ClsPurchaseVm
        //                {
        //                    TaxId = a.TaxId,
        //                    TaxAmount = a.TaxAmount,
        //                    PurchaseId = a.PurchaseId,
        //                    PurchaseDate = a.PurchaseDate,
        //                    ReferenceNo = a.ReferenceNo,
        //                    SupplierId = a.SupplierId,
        //                    SupplierName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.Name).FirstOrDefault(),
        //                    SupplierMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.MobileNo).FirstOrDefault(),
        //                    TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.SupplierId).Select(c => c.TaxNo).FirstOrDefault(),
        //                    Subtotal = a.Subtotal,
        //                    PaymentType = oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false
        //                    && b.PurchaseId == a.PurchaseId).Select(b => b.AccountId).Count() == 0 ? "" :
        //                 oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false
        //                 && b.PurchaseId == a.PurchaseId).Select(b => b.AccountId).Count() == 1 ?
        //                 oConnectionContext.DbClsSupplierPayment.Where(b => b.Type.ToLower() == "purchase payment" && b.IsDeleted == false && b.IsCancelled == false
        //                 && b.PurchaseId == a.PurchaseId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //                 ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //                    TotalDiscount = a.TotalDiscount,
        //                    Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                    { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                              join c in oConnectionContext.DbClsTaxMap
        //                                                                                              on b.TaxId equals c.TaxId
        //                                                                                              where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                              select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                           where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                           select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).ToList()
        //                }).ToList();

        //                purchaseTax2 = (from x in oConnectionContext.DbClsPurchaseDetails
        //                                join a in
        //oConnectionContext.DbClsPurchase on x.PurchaseId equals a.PurchaseId
        //                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && x.IsDeleted == false && x.IsCancelled == false
        //                        && a.BranchId == obj.BranchId
        //                         && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
        //                            DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate
        //                                select new ClsPurchaseVm
        //                                {
        //                                    TaxId = x.TaxId,
        //                                    TaxAmount = x.TaxAmount,
        //                                    PurchaseId = x.PurchaseId,
        //                                    PurchaseDate = a.PurchaseDate,
        //                                    ReferenceNo = a.ReferenceNo,
        //                                    SupplierId = a.SupplierId,
        //                                    SupplierName = "",
        //                                    SupplierMobileNo = "",
        //                                    TaxNo = "",
        //                                    Subtotal = x.PurchaseExcTax,
        //                                    PaymentType = "",
        //                                    TotalDiscount = a.TotalDiscount,
        //                                    Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == x.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                                    { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PurchaseExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                 join c in oConnectionContext.DbClsTaxMap
        //                                                                                                                                 on b.TaxId equals c.TaxId
        //                                                                                                                                 where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                                 select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * x.Quantity * x.PurchaseExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                                                                                 where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                                                                                 select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PurchaseExcTax }).ToList()
        //                                }).ToList();

        //                purchaseTax3 = purchaseTax1;

        //                foreach (var item in purchaseTax2)
        //                {
        //                    foreach (var tom in purchaseTax3.Where(w => w.PurchaseId == item.PurchaseId))
        //                    {
        //                        foreach (var inner in item.Taxs)
        //                        {
        //                            if (tom.Taxs.Where(w => w.TaxId == item.TaxId).Count() == 0)
        //                            {
        //                                tom.Taxs.Add(new ClsTaxVm { TaxId = inner.TaxId, TaxAmount = inner.TaxAmount });
        //                            }
        //                            else
        //                            {
        //                                decimal prevAmount = tom.Taxs.Where(a => a.TaxId == item.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                                foreach (var i in tom.Taxs.Where(w => w.TaxId == item.TaxId))
        //                                {
        //                                    i.TaxAmount = prevAmount + inner.TaxAmount;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            if (obj.UserId != 0)
        //            {
        //                purchaseTax3 = purchaseTax3.Where(a => a.SupplierId == obj.UserId).ToList();
        //            }

        //            var purchaseTax = (from a in purchaseTax3
        //                               select new
        //                               {
        //                                   a.TaxId,
        //                                   a.PurchaseId,
        //                                   a.PurchaseDate,
        //                                   a.ReferenceNo,
        //                                   a.SupplierId,
        //                                   a.SupplierName,
        //                                   a.SupplierMobileNo,
        //                                   a.TaxNo,
        //                                   a.Subtotal,
        //                                   a.PaymentType,
        //                                   a.TotalDiscount,
        //                                   Taxs = (from b in oConnectionContext.DbClsTax.AsEnumerable()
        //                                           where b.ForTaxGroupOnly == false && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        //                                           select new
        //                                           {
        //                                               b.TaxId,
        //                                               b.Tax,
        //                                               b.TaxPercent,
        //                                               IsTaxGroup = b.IsTaxGroup,
        //                                               TaxAmount = a.Taxs.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxAmount).FirstOrDefault(),
        //                                           }).Distinct().OrderBy(b => b.Tax).ToList()
        //                               }).ToList();

        //            PurchasePageCount = purchaseTax.Count();
        //            purchaseTax = purchaseTax.OrderByDescending(a => a.PurchaseId).Skip(skip).Take(obj.PageSize).ToList();

        //            List<ClsSalesVm> saleTax1;
        //            List<ClsSalesVm> saleTax2;
        //            List<ClsSalesVm> saleTax3 = new List<ClsSalesVm>();

        //            if (obj.BranchId == 0)
        //            {
        //                saleTax1 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //            //&& a.BranchId == obj.BranchId 
        //            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //             && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
        //                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate).Select(a => new ClsSalesVm
        //                {
        //                    TaxId = a.TaxId,
        //                    TaxAmount = a.TaxAmount,
        //                    SalesId = a.SalesId,
        //                    SalesDate = a.SalesDate,
        //                    InvoiceNo = a.InvoiceNo,
        //                    CustomerId = a.CustomerId,
        //                    CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
        //                    CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
        //                    TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.TaxNo).FirstOrDefault(),
        //                    Subtotal = a.Subtotal,
        //                    PaymentType = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment")
        //                    && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.AccountId).Count() == 0 ? "" :
        //                 oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
        //                 && b.SalesId == a.SalesId).Select(b => b.AccountId).Count() == 1 ?
        //                 oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
        //                 && b.SalesId == a.SalesId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //                 ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //                    TotalDiscount = a.TotalDiscount,
        //                    Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                    { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                              join c in oConnectionContext.DbClsTaxMap
        //                                                                                              on b.TaxId equals c.TaxId
        //                                                                                              where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                              select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                           where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                           select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).ToList()
        //                }).ToList();

        //                saleTax2 = (from x in oConnectionContext.DbClsSalesDetails
        //                            join a in
        //oConnectionContext.DbClsSales on x.SalesId equals a.SalesId
        //                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && x.IsDeleted == false && x.IsCancelled == false
        //                    //&& a.BranchId == obj.BranchId 
        //                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //                     && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
        //                        DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
        //                            select new ClsSalesVm
        //                            {
        //                                TaxId = x.TaxId,
        //                                TaxAmount = x.TaxAmount,
        //                                SalesId = x.SalesId,
        //                                SalesDate = a.SalesDate,
        //                                InvoiceNo = a.InvoiceNo,
        //                                CustomerId = a.CustomerId,
        //                                CustomerName = "",
        //                                CustomerMobileNo = "",
        //                                TaxNo = "",
        //                                Subtotal = x.PriceExcTax,
        //                                PaymentType = "",
        //                                TotalDiscount = a.TotalDiscount,
        //                                Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == x.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                                { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PriceExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                          join c in oConnectionContext.DbClsTaxMap
        //                                                                                                                          on b.TaxId equals c.TaxId
        //                                                                                                                          where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                          select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * x.Quantity * x.PriceExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                                                                       where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                                                                       select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PriceExcTax }).ToList()
        //                            }).ToList();

        //                saleTax3 = saleTax1;

        //                foreach (var item in saleTax2)
        //                {
        //                    foreach (var tom in saleTax3.Where(w => w.SalesId == item.SalesId))
        //                    {
        //                        foreach (var inner in item.Taxs)
        //                        {
        //                            if (tom.Taxs.Where(w => w.TaxId == item.TaxId).Count() == 0)
        //                            {
        //                                tom.Taxs.Add(new ClsTaxVm { TaxId = inner.TaxId, TaxAmount = inner.TaxAmount });
        //                            }
        //                            else
        //                            {
        //                                decimal prevAmount = tom.Taxs.Where(a => a.TaxId == item.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                                foreach (var i in tom.Taxs.Where(w => w.TaxId == item.TaxId))
        //                                {
        //                                    i.TaxAmount = prevAmount + inner.TaxAmount;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //            else
        //            {
        //                saleTax1 = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //             && a.BranchId == obj.BranchId
        //              && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
        //                 DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate).Select(a => new ClsSalesVm
        //                 {
        //                     TaxId = a.TaxId,
        //                     TaxAmount = a.TaxAmount,
        //                     SalesId = a.SalesId,
        //                     SalesDate = a.SalesDate,
        //                     InvoiceNo = a.InvoiceNo,
        //                     CustomerId = a.CustomerId,
        //                     CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
        //                     CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
        //                     TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.TaxNo).FirstOrDefault(),
        //                     Subtotal = a.Subtotal,
        //                     PaymentType = oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment")
        //                     && b.IsDeleted == false && b.IsCancelled == false && b.SalesId == a.SalesId).Select(b => b.AccountId).Count() == 0 ? "" :
        //                  oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
        //                  && b.SalesId == a.SalesId).Select(b => b.AccountId).Count() == 1 ?
        //                  oConnectionContext.DbClsCustomerPayment.Where(b => (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false
        //                  && b.SalesId == a.SalesId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //                  ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //                     TotalDiscount = a.TotalDiscount,
        //                     Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                     { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                               join c in oConnectionContext.DbClsTaxMap
        //                                                                                               on b.TaxId equals c.TaxId
        //                                                                                               where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                               select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.Subtotal }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                            where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                            select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.Subtotal }).ToList()
        //                 }).ToList();

        //                saleTax2 = (from x in oConnectionContext.DbClsSalesDetails
        //                            join a in
        //oConnectionContext.DbClsSales on x.SalesId equals a.SalesId
        //                            where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && x.IsDeleted == false && x.IsCancelled == false
        //                    && a.BranchId == obj.BranchId
        //                     && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
        //                        DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate
        //                            select new ClsSalesVm
        //                            {
        //                                TaxId = x.TaxId,
        //                                TaxAmount = x.TaxAmount,
        //                                SalesId = x.SalesId,
        //                                SalesDate = a.SalesDate,
        //                                InvoiceNo = a.InvoiceNo,
        //                                CustomerId = a.CustomerId,
        //                                CustomerName = "",
        //                                CustomerMobileNo = "",
        //                                TaxNo = "",
        //                                Subtotal = x.PriceExcTax,
        //                                PaymentType = "",
        //                                TotalDiscount = a.TotalDiscount,
        //                                Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == x.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                                { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PriceExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                          join c in oConnectionContext.DbClsTaxMap
        //                                                                                                                          on b.TaxId equals c.TaxId
        //                                                                                                                          where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                          select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * x.Quantity * x.PriceExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                                                                       where b.TaxId == x.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                                                                       select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * x.Quantity * x.PriceExcTax }).ToList()
        //                            }).ToList();

        //                saleTax3 = saleTax1;

        //                foreach (var item in saleTax2)
        //                {
        //                    foreach (var tom in saleTax3.Where(w => w.SalesId == item.SalesId))
        //                    {
        //                        foreach (var inner in item.Taxs)
        //                        {
        //                            if (tom.Taxs.Where(w => w.TaxId == item.TaxId).Count() == 0)
        //                            {
        //                                tom.Taxs.Add(new ClsTaxVm { TaxId = inner.TaxId, TaxAmount = inner.TaxAmount });
        //                            }
        //                            else
        //                            {
        //                                decimal prevAmount = tom.Taxs.Where(a => a.TaxId == item.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                                foreach (var i in tom.Taxs.Where(w => w.TaxId == item.TaxId))
        //                                {
        //                                    i.TaxAmount = prevAmount + inner.TaxAmount;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            if (obj.UserId != 0)
        //            {
        //                saleTax3 = saleTax3.Where(a => a.CustomerId == obj.UserId).ToList();
        //            }

        //            var saleTax = (from a in saleTax3
        //                           select new
        //                           {
        //                               a.TaxId,
        //                               a.SalesId,
        //                               a.SalesDate,
        //                               a.InvoiceNo,
        //                               a.CustomerId,
        //                               a.CustomerName,
        //                               a.CustomerMobileNo,
        //                               a.TaxNo,
        //                               a.Subtotal,
        //                               a.PaymentType,
        //                               a.TotalDiscount,
        //                               Taxs = (from b in oConnectionContext.DbClsTax.AsEnumerable()
        //                                       where b.ForTaxGroupOnly == false && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        //                                       select new
        //                                       {
        //                                           b.TaxId,
        //                                           b.Tax,
        //                                           b.TaxPercent,
        //                                           b.IsTaxGroup,
        //                                           TaxAmount = a.Taxs.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxAmount).FirstOrDefault(),
        //                                       }).Distinct().OrderBy(b => b.Tax).ToList()
        //                           }).ToList();

        //            SalePageCount = saleTax.Count();
        //            saleTax = saleTax.OrderByDescending(a => a.SalesId).Skip(skip).Take(obj.PageSize).ToList();

        //            List<ClsExpenseVm> expenseTax1;

        //            if (obj.BranchId == 0)
        //            {
        //                expenseTax1 = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //                        //&& a.BranchId == obj.BranchId 
        //                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
        //                        && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
        //                                DbFunctions.TruncateTime(a.Date) <= obj.ToDate).Select(a => new ClsExpenseVm
        //                                {
        //                                    //TaxId = a.TaxId,
        //                                    ExpenseId = a.ExpenseId,
        //                                    Date = a.Date,
        //                                    ReferenceNo = a.ReferenceNo,
        //                                    ExpenseFor = a.ExpenseFor,
        //                                    ExpenseForName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.ExpenseFor).Select(c => c.Name).FirstOrDefault(),
        //                                    ExpenseForMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.ExpenseFor).Select(c => c.MobileNo).FirstOrDefault(),
        //                                    TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.ExpenseFor).Select(c => c.TaxNo).FirstOrDefault(),
        //                                    //AmountExcTax=a.AmountExcTax,
        //                                    PaymentType = oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false
        //                                    && b.ExpenseId == a.ExpenseId).Select(b => b.AccountId).Count() == 0 ? "" :
        //            oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false
        //            && b.ExpenseId == a.ExpenseId).Select(b => b.AccountId).Count() == 1 ?
        //            oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false
        //            && b.ExpenseId == a.ExpenseId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //            ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //                                    Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                                    { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                  join c in oConnectionContext.DbClsTaxMap
        //                                                                                                                  on b.TaxId equals c.TaxId
        //                                                                                                                  where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                                                  select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                                                                                                                                                                                                                                                                                                                   where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                                                                                                                                                                                                                                                                                                                   select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax }).ToList()
        //                                }).ToList();
        //            }
        //            else
        //            {
        //                expenseTax1 = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //        && a.BranchId == obj.BranchId && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
        //                DbFunctions.TruncateTime(a.Date) <= obj.ToDate).Select(a => new ClsExpenseVm
        //                {
        //                    //TaxId=a.TaxId,
        //                    ExpenseId = a.ExpenseId,
        //                    Date = a.Date,
        //                    ReferenceNo = a.ReferenceNo,
        //                    ExpenseFor = a.ExpenseFor,
        //                    ExpenseForName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.ExpenseFor).Select(c => c.Name).FirstOrDefault(),
        //                    ExpenseForMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.ExpenseFor).Select(c => c.MobileNo).FirstOrDefault(),
        //                    TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.ExpenseFor).Select(c => c.TaxNo).FirstOrDefault(),
        //                    //AmountExcTax=a.AmountExcTax,
        //                    PaymentType = oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false
        //                    && b.Id == a.ExpenseId).Select(b => b.AccountId).Count() == 0 ? "" :
        //            oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false
        //            && b.Id == a.ExpenseId).Select(b => b.AccountId).Count() == 1 ?
        //            oConnectionContext.DbClsExpensePayment.Where(b => b.IsDeleted == false && b.IsCancelled == false
        //            && b.Id == a.ExpenseId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //            ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //                    //Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //                    //{ TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                    //                                                                                          join c in oConnectionContext.DbClsTaxMap
        //                    //                                                                                          on b.TaxId equals c.TaxId
        //                    //                                                                                          where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                    //                                                                                          select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //                    //                                                                                                                                                                                                                                                                                                       where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //                    //                                                                                                                                                                                                                                                                                                       select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax}).ToList()
        //                }).ToList();
        //            }


        //            //if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
        //            //{
        //            //    expenseTax1 = expenseTax1.Where(a => a.Date.Date >= obj.FromDate.AddHours(5).AddMinutes(30) &&
        //            //        a.Date.Date <= obj.ToDate.AddHours(5).AddMinutes(30)).ToList();
        //            //}

        //            if (obj.UserId != 0)
        //            {
        //                expenseTax1 = expenseTax1.Where(a => a.ExpenseFor == obj.UserId).ToList();
        //            }

        //            var expenseTax = (from a in expenseTax1
        //                              select new
        //                              {
        //                                  a.TaxId,
        //                                  a.ExpenseId,
        //                                  a.Date,
        //                                  a.ReferenceNo,
        //                                  a.ExpenseFor,
        //                                  a.ExpenseForName,
        //                                  a.ExpenseForMobileNo,
        //                                  a.TaxNo,
        //                                  a.AmountExcTax,
        //                                  a.PaymentType,
        //                                  Taxs = (from b in oConnectionContext.DbClsTax.AsEnumerable()
        //                                          where b.ForTaxGroupOnly == false && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        //                                          select new
        //                                          {
        //                                              b.TaxId,
        //                                              b.Tax,
        //                                              b.TaxPercent,
        //                                              b.IsTaxGroup,
        //                                              TaxAmount = a.Taxs.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxAmount).FirstOrDefault(),
        //                                          }).Distinct().OrderBy(b => b.Tax).ToList()
        //                              }).ToList();

        //            ExpensePageCount = expenseTax.Count();
        //            expenseTax = expenseTax.OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList();

        //            List<ClsIncomeVm> incomeTax1;

        //            //     if (obj.BranchId==0)
        //            //     {
        //            //         incomeTax1 = oConnectionContext.DbClsIncome.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //            ////&& a.BranchId == obj.BranchId
        //            //&& oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        //            // l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId) 
        //            //&& DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
        //            //         DbFunctions.TruncateTime(a.Date) <= obj.ToDate).Select(a => new ClsIncomeVm
        //            //         {
        //            //             TaxId=a.TaxId,
        //            //             IncomeId=a.IncomeId,
        //            //             Date=a.Date,
        //            //             ReferenceNo=a.ReferenceNo,
        //            //             IncomeFor=a.IncomeFor,
        //            //             IncomeForName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.IncomeFor).Select(c => c.Name).FirstOrDefault(),
        //            //             IncomeForMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.IncomeFor).Select(c => c.MobileNo).FirstOrDefault(),
        //            //             TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.IncomeFor).Select(c => c.TaxNo).FirstOrDefault(),
        //            //             AmountExcTax=a.AmountExcTax,
        //            //             PaymentType = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false
        //            //             && b.Id == a.IncomeId).Select(b => b.AccountId).Count() == 0 ? "" :
        //            //    oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false
        //            //    && b.Id == a.IncomeId).Select(b => b.AccountId).Count() == 1 ?
        //            //    oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false
        //            //    && b.Id == a.IncomeId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //            //    ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //            //             Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //            //             { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //            //                                                                                           join c in oConnectionContext.DbClsTaxMap
        //            //                                                                                           on b.TaxId equals c.TaxId
        //            //                                                                                           where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //            //                                                                                           select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //            //                                                                                                                                                                                                                                                                                            where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //            //                                                                                                                                                                                                                                                                                            select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax}).ToList()
        //            //         }).ToList();
        //            //     }
        //            //     else
        //            //     {
        //            //         incomeTax1 = oConnectionContext.DbClsIncome.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
        //            //&& a.BranchId == obj.BranchId && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
        //            //         DbFunctions.TruncateTime(a.Date) <= obj.ToDate).Select(a => new ClsIncomeVm
        //            //         {
        //            //             TaxId=a.TaxId,
        //            //             IncomeId=a.IncomeId,
        //            //             Date=a.Date,
        //            //             ReferenceNo=a.ReferenceNo,
        //            //             IncomeFor=a.IncomeFor,
        //            //             IncomeForName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.IncomeFor).Select(c => c.Name).FirstOrDefault(),
        //            //             IncomeForMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.IncomeFor).Select(c => c.MobileNo).FirstOrDefault(),
        //            //             TaxNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.IncomeFor).Select(c => c.TaxNo).FirstOrDefault(),
        //            //             AmountExcTax=a.AmountExcTax,
        //            //             PaymentType = oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false
        //            //             && b.Id == a.IncomeId).Select(b => b.AccountId).Count() == 0 ? "" :
        //            //    oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false
        //            //    && b.Id == a.IncomeId).Select(b => b.AccountId).Count() == 1 ?
        //            //    oConnectionContext.DbClsAccountsPayment.Where(b => b.Type.ToLower() == "income payment" && b.IsDeleted == false && b.IsCancelled == false
        //            //    && b.Id == a.IncomeId).Select(b => oConnectionContext.DbClsPaymentType.Where(c => c.PaymentTypeId == b.PaymentTypeId
        //            //    ).Select(c => c.PaymentType).FirstOrDefault()).FirstOrDefault() : "Multiple",
        //            //             Taxs = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxId && b.IsTaxGroup == false).Select(b => new ClsTaxVm
        //            //             { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //            //                                                                                           join c in oConnectionContext.DbClsTaxMap
        //            //                                                                                           on b.TaxId equals c.TaxId
        //            //                                                                                           where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //            //                                                                                           select new ClsTaxVm { TaxId = c.SubTaxId, TaxAmount = (oConnectionContext.DbClsTax.Where(e => e.TaxId == c.SubTaxId).Select(e => e.TaxPercent).FirstOrDefault() / 100) * a.AmountExcTax }).Union(from b in oConnectionContext.DbClsTax
        //            //                                                                                                                                                                                                                                                                                            where b.TaxId == a.TaxId && b.IsTaxGroup == true
        //            //                                                                                                                                                                                                                                                                                            select new ClsTaxVm { TaxId = b.TaxId, TaxAmount = (b.TaxPercent / 100) * a.AmountExcTax }).ToList()
        //            //         }).ToList();
        //            //     }


        //            //if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
        //            //{
        //            //    incomeTax1 = incomeTax1.Where(a => a.Date.Date >= obj.FromDate.AddHours(5).AddMinutes(30) &&
        //            //    a.Date.Date <= obj.ToDate.AddHours(5).AddMinutes(30)).ToList();
        //            //}

        //            //if (obj.UserId != 0)
        //            //{
        //            //    incomeTax1 = incomeTax1.Where(a => a.IncomeFor == obj.UserId).ToList();
        //            //}

        //            //var incomeTax = (from a in incomeTax1
        //            //                 select new
        //            //                 {
        //            //                     a.TaxId,
        //            //                     a.IncomeId,
        //            //                     a.Date,
        //            //                     a.ReferenceNo,
        //            //                     a.IncomeFor,
        //            //                     a.IncomeForName,
        //            //                     a.IncomeForMobileNo,
        //            //                     a.TaxNo,
        //            //                     a.AmountExcTax,
        //            //                     a.PaymentType,
        //            //                     Taxs = (from b in oConnectionContext.DbClsTax.AsEnumerable()
        //            //                             where b.ForTaxGroupOnly == false && b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        //            //                             select new
        //            //                             {
        //            //                                 b.TaxId,
        //            //                                 b.Tax,
        //            //                                 b.TaxPercent,
        //            //                                 b.IsTaxGroup,
        //            //                                 TaxAmount = a.Taxs.Where(c => c.TaxId == b.TaxId).Select(c => c.TaxAmount).FirstOrDefault(),
        //            //                             }).Distinct().OrderBy(b => b.Tax).ToList()
        //            //                 }).ToList();

        //            //IncomePageCount = incomeTax.Count();
        //            //incomeTax = incomeTax.OrderByDescending(a => a.IncomeId).Skip(skip).Take(obj.PageSize).ToList();

        //            decimal TotalPurchaseTax = 0, TotalSalesTax = 0, TotalExpenseTax = 0, TotalIncomeTax = 0;
        //            List<ClsPurchaseVm> oClsPurchaseVm = new List<ClsPurchaseVm>();

        //            foreach (var item in purchaseTax)
        //            {
        //                foreach (var inner in item.Taxs)
        //                {
        //                    if (oClsPurchaseVm.Where(a => a.TaxId == inner.TaxId).Count() == 0)
        //                    {
        //                        oClsPurchaseVm.Add(new ClsPurchaseVm { TaxId = inner.TaxId, Tax = inner.Tax, TaxAmount = inner.TaxAmount });
        //                    }
        //                    else
        //                    {
        //                        decimal prevTaxAmount = oClsPurchaseVm.Where(a => a.TaxId == inner.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                        foreach (var tom in oClsPurchaseVm.Where(w => w.TaxId == inner.TaxId))
        //                        {
        //                            tom.TaxAmount = prevTaxAmount + inner.TaxAmount;
        //                        }
        //                    }
        //                    if (inner.IsTaxGroup == false)
        //                    {
        //                        TotalPurchaseTax = TotalPurchaseTax + inner.TaxAmount;
        //                    }
        //                }
        //            }

        //            List<ClsSalesVm> oClsSalesVm = new List<ClsSalesVm>();
        //            foreach (var item in saleTax)
        //            {
        //                foreach (var inner in item.Taxs)
        //                {
        //                    if (oClsSalesVm.Where(a => a.TaxId == inner.TaxId).Count() == 0)
        //                    {
        //                        oClsSalesVm.Add(new ClsSalesVm { TaxId = inner.TaxId, Tax = inner.Tax, TaxAmount = inner.TaxAmount });
        //                    }
        //                    else
        //                    {
        //                        decimal prevTaxAmount = oClsSalesVm.Where(a => a.TaxId == inner.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                        foreach (var tom in oClsSalesVm.Where(w => w.TaxId == inner.TaxId))
        //                        {
        //                            tom.TaxAmount = prevTaxAmount + inner.TaxAmount;
        //                        }
        //                    }
        //                    if (inner.IsTaxGroup == false)
        //                    {
        //                        TotalSalesTax = TotalSalesTax + inner.TaxAmount;
        //                    }
        //                }
        //            }

        //            List<ClsExpenseVm> oClsExpenseVm = new List<ClsExpenseVm>();
        //            foreach (var item in expenseTax)
        //            {
        //                foreach (var inner in item.Taxs)
        //                {
        //                    if (oClsExpenseVm.Where(a => a.TaxId == inner.TaxId).Count() == 0)
        //                    {
        //                        oClsExpenseVm.Add(new ClsExpenseVm { TaxId = inner.TaxId, Tax = inner.Tax, TaxAmount = inner.TaxAmount });
        //                    }
        //                    else
        //                    {
        //                        decimal prevTaxAmount = oClsExpenseVm.Where(a => a.TaxId == inner.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //                        foreach (var tom in oClsExpenseVm.Where(w => w.TaxId == inner.TaxId))
        //                        {
        //                            tom.TaxAmount = prevTaxAmount + inner.TaxAmount;
        //                        }
        //                    }
        //                    if (inner.IsTaxGroup == false)
        //                    {
        //                        TotalExpenseTax = TotalExpenseTax + inner.TaxAmount;
        //                    }
        //                }
        //            }

        //            List<ClsIncomeVm> oClsIncomeVm = new List<ClsIncomeVm>();
        //            //foreach (var item in incomeTax)
        //            //{
        //            //    foreach (var inner in item.Taxs)
        //            //    {
        //            //        if (oClsIncomeVm.Where(a => a.TaxId == inner.TaxId).Count() == 0)
        //            //        {
        //            //            oClsIncomeVm.Add(new ClsIncomeVm { TaxId = inner.TaxId, Tax = inner.Tax, TaxAmount = inner.TaxAmount });
        //            //        }
        //            //        else
        //            //        {
        //            //            decimal prevTaxAmount = oClsIncomeVm.Where(a => a.TaxId == inner.TaxId).Select(a => a.TaxAmount).FirstOrDefault();
        //            //            foreach (var tom in oClsIncomeVm.Where(w => w.TaxId == inner.TaxId))
        //            //            {
        //            //                tom.TaxAmount = prevTaxAmount + inner.TaxAmount;
        //            //            }
        //            //        }
        //            //        if (inner.IsTaxGroup == false)
        //            //        {
        //            //            TotalIncomeTax = TotalIncomeTax + inner.TaxAmount;
        //            //        }
        //            //    }
        //            //}

        //            data = new
        //            {
        //                Status = 1,
        //                Message = "found",
        //                Data = new
        //                {
        //                    TaxReport = new
        //                    {
        //                        Purchases = purchaseTax,//.OrderByDescending(a => a.PurchaseId).Skip(skip).Take(obj.PageSize).ToList(),
        //                        Sales = saleTax,//.OrderByDescending(a => a.SalesId).Skip(skip).Take(obj.PageSize).ToList(),
        //                        Expenses = expenseTax,//.OrderByDescending(a => a.ExpenseId).Skip(skip).Take(obj.PageSize).ToList(),
        //                        //Incomes = incomeTax,//.OrderByDescending(a => a.IncomeId).Skip(skip).Take(obj.PageSize).ToList(),
        //                        TotalPurchaseCount = PurchasePageCount,
        //                        TotalSaleCount = SalePageCount,
        //                        TotalExpenseCount = ExpensePageCount,
        //                        TotalIncomeCount = IncomePageCount,
        //                        OverallTax = (TotalSalesTax + TotalIncomeTax) - (TotalPurchaseTax + TotalExpenseTax)
        //                    },
        //                    Purchases = oClsPurchaseVm,
        //                    Sales = oClsSalesVm,
        //                    Expenses = oClsExpenseVm,
        //                    Incomes = oClsIncomeVm,
        //                    //Branchs = userDetails.BranchIds,
        //                    PageSize = obj.PageSize,
        //                    FromDate = obj.FromDate,
        //                    ToDate = obj.ToDate,
        //                }
        //            };

        //            return await Task.FromResult(Ok(data));
        //        }

        public async Task<IHttpActionResult> TaxSummaryReport(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();

            Transactions = oCommonController.TaxTransactions(obj);

            var det = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
             //&& a.ForTaxGroupOnly == false
             && a.IsTaxGroup == false
             && a.CanDelete == true
             ).AsEnumerable().Select(a => new
             {
                 TaxId = a.TaxId,
                 a.TaxPercent,
                 Tax = a.Tax,
                 TaxableAmount = Transactions.Where(c => c.TaxId == a.TaxId).Select(c => c.TransactionAmount).DefaultIfEmpty().Sum(),
                 TaxAmount = Transactions.Where(c => c.TaxId == a.TaxId).Select(c => c.TaxAmount).DefaultIfEmpty().Sum(),
             }).OrderBy(a => a.Tax).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Taxs = det,
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> TaxSummaryDetailsReport(ClsBankPaymentVm obj)
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

            List<ClsBankPaymentVm> Transactions = new List<ClsBankPaymentVm>();

            Transactions = oCommonController.TaxTransactions(obj).Where(a => a.TaxId == obj.TaxId).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BankPayments = Transactions.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = Transactions.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        #region GSTR 1
        public async Task<IHttpActionResult> B2B(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.B2B(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Sales.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Sales.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Sales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateB2BJson(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.B2B(obj);

            var result = det.GroupedSales
                .GroupBy(x => x.BusinessRegistrationNo) // ctin
                .Select(g => new
                {
                    ctin = g.Key,
                    inv = g.Select(inv => new
                    {
                        inum = inv.InvoiceNo,
                        idt = inv.SalesDate.ToString("dd-MM-yyyy"),
                        val = inv.GrandTotal,
                        pos = inv.StateCode, // should be like "29"
                        rchrg = inv.IsReverseCharge,
                        inv_typ = inv.InvoiceType == "Regular B2B" ? "R" : inv.InvoiceType == "SEZ supplies with payment" ? "SEWP" :
                        inv.InvoiceType == "SEZ supplies without payment" ? "SEWOP" : inv.InvoiceType == "Deemed Export" ? "DE" : "R",
                        itms = inv.SalesDetails.Select((item, index) => new
                        {
                            num = index + 1,
                            itm_det = new
                            {
                                txval = item.AmountExcTax,
                                rt = item.TaxPercent,
                                iamt = item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount),
                                camt = item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount),
                                samt = item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount),
                                csamt = item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount)
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                b2b = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> B2CLarge(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.B2CLarge(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Sales.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Sales.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Sales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateB2CLargeJson(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.B2CLarge(obj);

            var result = det.GroupedSales
                .GroupBy(x => x.StateCode)
                .Select(g => new
                {
                    pos = g.Key,
                    inv = g.Select(inv => new
                    {
                        inum = inv.InvoiceNo,
                        idt = inv.SalesDate.ToString("dd-MM-yyyy"),
                        val = Math.Round(inv.GrandTotal, 2),
                        itms = inv.SalesDetails.Select((item, index) => new
                        {
                            num = index + 1,
                            itm_det = new
                            {
                                txval = Math.Round(item.AmountExcTax, 2),
                                rt = item.TaxPercent,
                                iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                                camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                                samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                                csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2),
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                b2cl = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> B2CSmall(ClsSalesVm obj)
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

            ClsGstSale det = oGstController.B2CSmall(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSalesDetails.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.SalesDetails.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SalesDetails.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.SalesDetails.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateB2CSmallJson(ClsSalesVm obj)
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

            ClsGstSale det = oGstController.B2CSmall(obj);

            var result = det.SalesDetails
                .GroupBy(x => new { x.SupplyType, x.StateCode, x.TaxPercent })
                    .Select(g => new
                    {
                        sply_ty = g.Key.SupplyType,
                        pos = g.Key.StateCode,
                        typ = "OE", // Stands for Other Exempted - Change if needed
                        txval = Math.Round(g.Sum(x => x.AmountExcTax), 2),
                        rt = g.Key.TaxPercent,
                        iamt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)), 2),
                        camt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)), 2),
                        samt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount)), 2),
                        csamt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount)), 2)
                    })
                .ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                b2cs = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> Exports(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.Exports(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Sales.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Sales.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Sales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }
        public async Task<IHttpActionResult> GenerateExportsJson(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.Exports(obj);

            var result = det.GroupedSales
                .GroupBy(x => x.InvoiceType) // WPAY / WOPAY
                .Select(g => new
                {
                    exp_typ = g.Key, // "WPAY" or "WOPAY"
                    inv = g.Select(inv => new
                    {
                        inum = inv.InvoiceNo,
                        idt = inv.SalesDate.ToString("dd-MM-yyyy"),
                        val = inv.GrandTotal,
                        sbpcode = inv.PortCode,
                        sbnum = inv.ShippingBillNo,
                        sbdt = inv.ShippingBillDate.ToString("dd-MM-yyyy"),
                        itms = inv.SalesDetails.Select(item => new
                        {
                            txval = item.AmountExcTax,
                            rt = item.TaxPercent,
                            iamt = item.TaxTypes
                                        .Where(t => t.TaxType == "IGST")
                                        .Sum(t => t.TaxAmount),
                            csamt = item.TaxTypes
                                        .Where(t => t.TaxType == "CESS")
                                        .Sum(t => t.TaxAmount),
                        }).ToList()
                    }).ToList()
                }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                exp = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> CreditDebitRegistered(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.CreditDebitRegistered(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Sales.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Sales.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Sales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateCreditDebitRegisteredJson(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.CreditDebitRegistered(obj);

            var result = det.GroupedSales
                .GroupBy(x => x.BusinessRegistrationNo)
                .Select(g => new
                {
                    ctin = g.Key,
                    nt = g.Select(n => new
                    {
                        nt_num = n.InvoiceNo,
                        nt_dt = n.SalesDate.ToString("dd-MM-yyyy"),
                        ntty = n.PaymentType == "credit" ? "C" : "D",
                        val = Math.Round(n.GrandTotal, 2),
                        pos = n.StateCode,
                        rchrg = "Y",
                        inv_typ = n.InvoiceType == "Regular B2B" ? "R" : n.InvoiceType == "SEZ supplies with payment" ? "SEWP" :
                        n.InvoiceType == "SEZ supplies without payment" ? "SEWOP" : n.InvoiceType == "Deemed Export" ? "DE" : "R",
                        itms = n.SalesDetails.Select((item, index) => new
                        {
                            num = index + 1,
                            itm_det = new
                            {
                                txval = Math.Round(item.AmountExcTax, 2),
                                rt = item.TaxPercent,
                                iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                                camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                                samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                                csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2),
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                cdnr = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> CreditDebitUnRegistered(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.CreditDebitUnRegistered(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Sales.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Sales.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.Sales.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Sales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateCreditDebitUnRegisteredJson(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.CreditDebitUnRegistered(obj);

            var result = det.GroupedSales.Select(x => new
            {
                nt_num = x.InvoiceNo,
                nt_dt = x.SalesDate.ToString("dd-MM-yyyy"),
                ntty = x.SalesType == "Credit Note" ? "C" : "D",
                val = x.GrandTotal,
                typ = x.InvoiceType,
                pos = x.StateCode,
                //diff_percent = (x.SalesDetails.Any(d => d.TaxTypes.Sum(t => t.TaxAmount) < (d.AmountExcTax * (d.TaxPercent / 100))) ? 0.65 : (double?)null),
                itms = x.SalesDetails.Select((itm, index) => new
                {
                    num = (index + 1),
                    itm_det = new
                    {
                        txval = itm.AmountExcTax,
                        rt = itm.TaxPercent,
                        iamt = itm.TaxTypes.Sum(t => t.TaxType == "IGST" ? t.TaxAmount : 0),
                        camt = itm.TaxTypes.Sum(t => t.TaxType == "CGST" ? t.TaxAmount : 0),
                        samt = itm.TaxTypes.Sum(t => (t.TaxType == "SGST" || t.TaxType == "UTGST") ? t.TaxAmount : 0),
                        csamt = itm.TaxTypes.Sum(t => t.TaxType == "Cess" ? t.TaxAmount : 0)
                    }
                }).ToList()
            }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                cdnur = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> AdvancesReceived(ClsSalesVm obj)
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

            ClsGstSale det = oGstController.AdvancesReceived(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det.GroupedSalesDetails.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.SalesDetails.Select(a => a.AmountRemaining).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SalesDetails.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.SalesDetails.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateAdvancesReceivedJson(ClsSalesVm obj)
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

            ClsGstSale det = oGstController.AdvancesReceived(obj);

            var result = det.GroupedSalesDetails
                .GroupBy(x => new { x.StateCode, x.SupplyType })
                .Select(g => new
                {
                    pos = g.Key.StateCode,
                    sply_ty = g.Key.SupplyType,
                    itms = g.Select(item => new
                    {
                        rt = item.TaxPercent,
                        ad_amt = Math.Round(item.AmountExcTax, 2),
                        iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                        camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                        samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                        csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2)
                    }).ToList()
                }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                at = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> AdjustmentOfAdvances(ClsSalesVm obj)
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

            ClsGstSale det = oGstController.AdjustmentOfAdvances(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CustomerPayments = det.GroupedSalesDetails.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.SalesDetails.Select(a => a.Amount).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SalesDetails.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.SalesDetails.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateAdjustmentOfAdvancesJson(ClsSalesVm obj)
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

            ClsGstSale det = oGstController.AdjustmentOfAdvances(obj);

            var result = det.GroupedSalesDetails
                .GroupBy(x => new { x.StateCode, x.SupplyType })
                .Select(g => new
                {
                    pos = g.Key.StateCode,
                    sply_ty = g.Key.SupplyType,
                    itms = g.Select(item => new
                    {
                        rt = item.TaxPercent,
                        ad_amt = Math.Round(item.AmountExcTax, 2),
                        iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                        camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                        samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                        csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2)
                    }).ToList()
                }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                txpd = result
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> HsnWiseSummary(ClsSalesVm obj)
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


            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = oGstController.HsnWiseSummary(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    SalesDetails = det.GroupedSalesDetails.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.SalesDetails.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SalesDetails.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.SalesDetails.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateHsnWiseSummaryJson(ClsSalesVm obj)
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

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = oGstController.HsnWiseSummary(obj);

            var result = det.GroupedSalesDetails.Select((x, index) => new
            {
                num = index + 1,
                hsn_sc = x.Code,
                desc = x.Description,
                uqc = x.UnitShortName,
                qty = x.Quantity,
                rt = x.TaxPercent,
                txval = x.AmountExcTax,
                iamt = x.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount),
                camt = x.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount),
                samt = x.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount),
                csamt = x.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount)
            }).ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                hsn = new
                {
                    data = result
                }
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> NilRated(ClsSalesVm obj)
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

            var det = oGstController.NilRated(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    //SalesDetails = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    SalesDetails = det.SalesDetails,
                    TotalNilAmount = det.NilRated.Select(a => a.NilRatedAmount).DefaultIfEmpty().Sum(),
                    TotalExemptedAmount = det.Exempted.Select(a => a.ExemptedAmount).DefaultIfEmpty().Sum(),
                    TotalNonGstAmount = det.NonGst.Select(a => a.NonGstAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.SalesDetails.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateNilRatedJson(ClsSalesVm obj)
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

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = oGstController.NilRated(obj);

            var splyTypeMap = new Dictionary<string, string>
                {
                    { "Inter-State supplies to registered persons", "INTRB2B" },
                    { "Intra-State supplies to registered persons", "INTRAB2B" },
                    { "Inter-State supplies to unregistered persons", "INTRB2C" },
                    { "Intra-State supplies to unregistered persons", "INTRAB2C" }
                };

            var result = det.SalesDetails
                .Where(s => splyTypeMap.ContainsKey(s.Description))
                .Select(s => new
                {
                    sply_ty = splyTypeMap[s.Description],
                    expt_amt = s.ExemptedAmount,
                    nil_amt = s.NilRatedAmount,
                    ngsup_amt = s.NonGstAmount
                })
                .ToList();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                nil = new
                {
                    inv = result
                }
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> SummaryOfDocuments(ClsSalesVm obj)
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

            var det = oGstController.SummaryOfDocuments(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TaxDocuments = det.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateSummaryOfDocumentsJson(ClsSalesVm obj)
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

            var det = oGstController.SummaryOfDocuments(obj);

            var result = new List<object>();

            foreach (var group in det.GroupBy(x => new { x.DocumentNumber, x.NatureOfDocument }))
            {
                int docDetailCounter = 1;

                var formatted = new
                {
                    doc_num = group.Key.DocumentNumber,
                    doc_typ = group.Key.NatureOfDocument,
                    docs = group.Select(x => new
                    {
                        num = docDetailCounter++,
                        from = x.From,
                        to = x.To,
                        totnum = x.TotalNumber,
                        cancel = x.TotalCancelled,
                        net_issue = x.TotalNumber - x.TotalCancelled
                    }).ToList()
                };

                result.Add(formatted);
            }


            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                doc_issue = new
                {
                    doc_det = result
                }
            };

            return await Task.FromResult(Ok(json));
        }

        public async Task<IHttpActionResult> GenerateGstr1Json(ClsSalesVm obj)
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

            #region B2B
            var B2B = oGstController.B2B(obj);
            var B2BResult = B2B.GroupedSales
                .GroupBy(x => x.BusinessRegistrationNo) // ctin
                .Select(g => new
                {
                    ctin = g.Key,
                    inv = g.Select(inv => new
                    {
                        inum = inv.InvoiceNo,
                        idt = inv.SalesDate.ToString("dd-MM-yyyy"),
                        val = inv.GrandTotal,
                        pos = inv.StateCode, // should be like "29"
                        rchrg = inv.IsReverseCharge,
                        inv_typ = inv.InvoiceType == "Regular B2B" ? "R" : inv.InvoiceType == "Deemed Export" ? "DE" : "R",
                        itms = inv.SalesDetails.Select((item, index) => new
                        {
                            num = index + 1,
                            itm_det = new
                            {
                                txval = item.AmountExcTax,
                                rt = item.TaxPercent,
                                iamt = item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount),
                                camt = item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount),
                                samt = item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount),
                                csamt = item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount)
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();
            #endregion

            #region B2CLarge
            ClsGstResult B2CLarge = oGstController.B2CLarge(obj);

            var B2CLargeResult = B2CLarge.GroupedSales
                .GroupBy(x => x.StateCode)
                .Select(g => new
                {
                    pos = g.Key,
                    inv = g.Select(inv => new
                    {
                        inum = inv.InvoiceNo,
                        idt = inv.SalesDate.ToString("dd-MM-yyyy"),
                        val = Math.Round(inv.GrandTotal, 2),
                        itms = inv.SalesDetails.Select((item, index) => new
                        {
                            num = index + 1,
                            itm_det = new
                            {
                                txval = Math.Round(item.AmountExcTax, 2),
                                rt = item.TaxPercent,
                                iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                                camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                                samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                                csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2),
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();
            #endregion

            #region B2CSmall 
            var B2CSmall = oGstController.B2CSmall(obj);
            var B2CSmallResult = B2CSmall.SalesDetails
               .GroupBy(x => new { x.SupplyType, x.StateCode, x.TaxPercent })
                   .Select(g => new
                   {
                       sply_ty = g.Key.SupplyType,
                       pos = g.Key.StateCode,
                       typ = "OE", // Stands for Other Exempted - Change if needed
                       txval = Math.Round(g.Sum(x => x.AmountExcTax), 2),
                       rt = g.Key.TaxPercent,
                       iamt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)), 2),
                       camt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)), 2),
                       samt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount)), 2),
                       csamt = Math.Round(g.Sum(x => x.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount)), 2)
                   })
               .ToList();
            #endregion

            #region Exports
            var Exports = oGstController.Exports(obj);
            var ExportsResult = Exports.GroupedSales
                .GroupBy(x => x.InvoiceType) // WPAY / WOPAY
                .Select(g => new
                {
                    exp_typ = g.Key, // "WPAY" or "WOPAY"
                    inv = g.Select(inv => new
                    {
                        inum = inv.InvoiceNo,
                        idt = inv.SalesDate.ToString("dd-MM-yyyy"),
                        val = inv.GrandTotal,
                        sbpcode = inv.PortCode,
                        sbnum = inv.ShippingBillNo,
                        sbdt = inv.ShippingBillDate.ToString("dd-MM-yyyy"),
                        itms = inv.SalesDetails.Select(item => new
                        {
                            txval = item.AmountExcTax,
                            rt = item.TaxPercent,
                            iamt = item.TaxTypes
                                        .Where(t => t.TaxType == "IGST")
                                        .Sum(t => t.TaxAmount),
                            csamt = item.TaxTypes
                                        .Where(t => t.TaxType == "CESS")
                                        .Sum(t => t.TaxAmount),
                        }).ToList()
                    }).ToList()
                }).ToList();
            #endregion

            #region CreditDebitRegistered
            var CreditDebitRegistered = oGstController.CreditDebitRegistered(obj);
            var CreditDebitRegisteredResult = CreditDebitRegistered.GroupedSales
                .GroupBy(x => x.BusinessRegistrationNo)
                .Select(g => new
                {
                    ctin = g.Key,
                    nt = g.Select(n => new
                    {
                        nt_num = n.InvoiceNo,
                        nt_dt = n.SalesDate.ToString("dd-MM-yyyy"),
                        ntty = n.PaymentType == "credit" ? "C" : "D",
                        val = Math.Round(n.GrandTotal, 2),
                        pos = n.StateCode,
                        rchrg = "Y",
                        inv_typ = "R",
                        itms = n.SalesDetails.Select((item, index) => new
                        {
                            num = index + 1,
                            itm_det = new
                            {
                                txval = Math.Round(item.AmountExcTax, 2),
                                rt = item.TaxPercent,
                                iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                                camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                                samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                                csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2),
                            }
                        }).ToList()
                    }).ToList()
                }).ToList();
            #endregion

            #region CreditDebitUnRegistered
            var CreditDebitUnRegistered = oGstController.CreditDebitUnRegistered(obj);
            var CreditDebitUnRegisteredResult = CreditDebitUnRegistered.GroupedSales.Select(x => new
            {
                nt_num = x.InvoiceNo,
                nt_dt = x.SalesDate.ToString("dd-MM-yyyy"),
                ntty = x.SalesType == "Credit Note" ? "C" : "D",
                val = x.GrandTotal,
                typ = "B2CL",
                pos = x.StateCode,
                //diff_percent = (x.SalesDetails.Any(d => d.TaxTypes.Sum(t => t.TaxAmount) < (d.AmountExcTax * (d.TaxPercent / 100))) ? 0.65 : (double?)null),
                itms = x.SalesDetails.Select((itm, index) => new
                {
                    num = (index + 1),
                    itm_det = new
                    {
                        txval = itm.AmountExcTax,
                        rt = itm.TaxPercent,
                        iamt = itm.TaxTypes.Sum(t => t.TaxType == "IGST" ? t.TaxAmount : 0),
                        camt = itm.TaxTypes.Sum(t => t.TaxType == "CGST" ? t.TaxAmount : 0),
                        samt = itm.TaxTypes.Sum(t => (t.TaxType == "SGST" || t.TaxType == "UTGST") ? t.TaxAmount : 0),
                        csamt = itm.TaxTypes.Sum(t => t.TaxType == "Cess" ? t.TaxAmount : 0)
                    }
                }).ToList()
            }).ToList();
            #endregion

            #region AdvancesReceived
            var AdvancesReceived = oGstController.AdvancesReceived(obj);
            var AdvancesReceivedResult = AdvancesReceived.GroupedSalesDetails
                .GroupBy(x => new { x.StateCode, x.SupplyType })
                .Select(g => new
                {
                    pos = g.Key.StateCode,
                    sply_ty = g.Key.SupplyType,
                    itms = g.Select(item => new
                    {
                        rt = item.TaxPercent,
                        ad_amt = Math.Round(item.AmountExcTax, 2),
                        iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                        camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                        samt = Math.Round(item.TaxTypes.Where(t => (t.TaxType == "SGST" || t.TaxType == "UTGST")).Sum(t => t.TaxAmount), 2),
                        csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2)
                    }).ToList()
                }).ToList();
            #endregion

            #region AdjustmentOfAdvances
            var AdjustmentOfAdvances = oGstController.AdjustmentOfAdvances(obj);
            var AdjustmentOfAdvancesResult = AdjustmentOfAdvances.GroupedSalesDetails
                .GroupBy(x => new { x.StateCode, x.SupplyType })
                .Select(g => new
                {
                    pos = g.Key.StateCode,
                    sply_ty = g.Key.SupplyType,
                    itms = g.Select(item => new
                    {
                        rt = item.TaxPercent,
                        ad_amt = Math.Round(item.AmountExcTax, 2),
                        iamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount), 2),
                        camt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount), 2),
                        samt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount), 2),
                        csamt = Math.Round(item.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount), 2)
                    }).ToList()
                }).ToList();
            #endregion

            #region HsnWiseSummary
            var HsnWiseSummary = oGstController.HsnWiseSummary(obj);
            var HsnWiseSummaryResult = HsnWiseSummary.GroupedSalesDetails.Select((x, index) => new
            {
                num = index + 1,
                hsn_sc = x.Code,
                desc = x.Description,
                uqc = x.UnitShortName,
                qty = x.Quantity,
                rt = x.TaxPercent,
                txval = x.AmountExcTax,
                iamt = x.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount),
                camt = x.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount),
                samt = x.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount),
                csamt = x.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount)
            }).ToList();
            #endregion

            #region NilRated
            var NilRated = oGstController.NilRated(obj);
            var splyTypeMap = new Dictionary<string, string>
                {
                    { "Inter-State supplies to registered persons", "INTRB2B" },
                    { "Intra-State supplies to registered persons", "INTRAB2B" },
                    { "Inter-State supplies to unregistered persons", "INTRB2C" },
                    { "Intra-State supplies to unregistered persons", "INTRAB2C" }
                };

            var NilRatedResult = NilRated.SalesDetails
                .Where(s => splyTypeMap.ContainsKey(s.Description))
                .Select(s => new
                {
                    sply_ty = splyTypeMap[s.Description],
                    expt_amt = s.ExemptedAmount,
                    nil_amt = s.NilRatedAmount,
                    ngsup_amt = s.NonGstAmount
                })
                .ToList();
            #endregion

            #region SummaryOfDocuments
            var SummaryOfDocuments = oGstController.SummaryOfDocuments(obj);
            var SummaryOfDocumentsResult = new List<object>();

            foreach (var group in SummaryOfDocuments.GroupBy(x => new { x.DocumentNumber, x.NatureOfDocument }))
            {
                int docDetailCounter = 1;

                var formatted = new
                {
                    doc_num = group.Key.DocumentNumber,
                    doc_typ = group.Key.NatureOfDocument,
                    docs = group.Select(x => new
                    {
                        num = docDetailCounter++,
                        from = x.From,
                        to = x.To,
                        totnum = x.TotalNumber,
                        cancel = x.TotalCancelled,
                        net_issue = x.TotalNumber - x.TotalCancelled
                    }).ToList()
                };

                SummaryOfDocumentsResult.Add(formatted);
            }
            #endregion

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = obj.ToDate.Month.ToString("D2") + obj.ToDate.Year.ToString(),
                version = "GST3.2.1",
                hash = "hash",
                b2b = B2BResult,
                b2cl = B2CLargeResult,
                b2cs = B2CSmallResult,
                exp = ExportsResult,
                cdnr = CreditDebitRegisteredResult,
                cdnur = CreditDebitUnRegisteredResult,
                at = AdvancesReceivedResult,
                txpd = AdjustmentOfAdvancesResult,
                hsn = new
                {
                    data = HsnWiseSummaryResult
                },
                nil = new
                {
                    inv = NilRatedResult
                },
                doc_issue = new
                {
                    doc_det = SummaryOfDocumentsResult
                }
            };

            return await Task.FromResult(Ok(json));
        }

        #endregion

        #region GSTR 3B
        public async Task<IHttpActionResult> OutwardTaxable(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.OutwardTaxable(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> OutwardTaxableZeroRated(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.OutwardTaxableZeroRated(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> OtherOutwardNilRated(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.OtherOutwardNilRated(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardReverseCharge(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InwardReverseCharge(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> OutwardNonGst(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.OutwardNonGst(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InterStateUnregistered(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InterStateUnregistered(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    //TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    //TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InterStateComposition(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InterStateComposition(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    //TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    //TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportOfGoods(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.ImportOfGoods(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportOfServices(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.ImportOfServices(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardLiableToReverseCharge(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InwardLiableToReverseCharge(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllOtherItc(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.AllOtherItc(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.GroupedSales.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.GroupedSales.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.GroupedSales.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardInterComposition(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InwardInterComposition(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    //TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    //TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardIntraComposition(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InwardIntraComposition(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    //TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    //TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardInterNonGst(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InwardInterNonGst(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    //TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    //TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardIntraNonGst(ClsSalesVm obj)
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

            ClsGstResult det = oGstController.InwardIntraNonGst(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.GroupedSales.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    //TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    //TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    //TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateGstr3BJson(ClsSalesVm obj)
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

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxId == oConnectionContext.DbClsTaxSetting.Where(b => b.CompanyId == obj.CompanyId && b.TaxSettingId == obj.TaxSettingId).Select(b => b.CompositionSchemeTaxId).FirstOrDefault()
            ).Select(a => a.TaxPercent).FirstOrDefault();

            ClsGstResult OutwardTaxable = oGstController.OutwardTaxable(obj);
            var osup_det = new
            {
                txval = OutwardTaxable.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                iamt = OutwardTaxable.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = OutwardTaxable.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = OutwardTaxable.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = OutwardTaxable.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult OutwardTaxableZeroRated = oGstController.OutwardTaxableZeroRated(obj);
            var osup_zero = new
            {
                txval = OutwardTaxableZeroRated.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                iamt = OutwardTaxableZeroRated.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = OutwardTaxableZeroRated.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = OutwardTaxableZeroRated.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = OutwardTaxableZeroRated.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult OtherOutwardNilRated = oGstController.OtherOutwardNilRated(obj);
            var osup_nil_exmp = new
            {
                txval = OtherOutwardNilRated.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                iamt = OtherOutwardNilRated.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = OtherOutwardNilRated.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = OtherOutwardNilRated.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = OtherOutwardNilRated.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult InwardReverseCharge = oGstController.InwardReverseCharge(obj);
            var isup_rev = new
            {
                txval = InwardReverseCharge.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                iamt = InwardReverseCharge.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = InwardReverseCharge.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = InwardReverseCharge.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = InwardReverseCharge.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult OutwardNonGst = oGstController.OutwardNonGst(obj);
            var osup_nongst = new
            {
                txval = OutwardNonGst.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                iamt = OutwardNonGst.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = OutwardNonGst.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = OutwardNonGst.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = OutwardNonGst.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult InterStateUnregistered = oGstController.InterStateUnregistered(obj);
            var unreg_details = InterStateUnregistered.GroupedSales.GroupBy(s => new { s.StateCode })
            .Select(g => new
            {
                pos = g.Key.StateCode,
                txval = g.Select(b => b.AmountExcTax).Sum(),
                iamt = g.Select(b => b.TotalIgstValue).Sum(),
            }).ToList();

            ClsGstResult InterStateComposition = oGstController.InterStateComposition(obj);
            var comp_details = InterStateComposition.GroupedSales.GroupBy(s => new { s.StateCode })
            .Select(g => new
            {
                pos = g.Key.StateCode,
                txval = g.Select(b => b.AmountExcTax).Sum(),
                iamt = g.Select(b => b.TotalIgstValue).Sum(),
            }).ToList();

            ClsGstResult ImportOfGoods = oGstController.ImportOfGoods(obj);
            var IMPG = new
            {
                ty = "IMPG",
                iamt = ImportOfGoods.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = ImportOfGoods.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = ImportOfGoods.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = ImportOfGoods.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult ImportOfServices = oGstController.ImportOfServices(obj);
            var IMPS = new
            {
                ty = "IMPS",
                iamt = ImportOfServices.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = ImportOfServices.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = ImportOfServices.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = ImportOfServices.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult InwardLiableToReverseCharge = oGstController.InwardLiableToReverseCharge(obj);
            var ISRC = new
            {
                ty = "ISRC",
                iamt = InwardLiableToReverseCharge.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = InwardLiableToReverseCharge.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = InwardLiableToReverseCharge.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = InwardLiableToReverseCharge.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult AllOtherItc = oGstController.AllOtherItc(obj);
            var OTH = new
            {
                ty = "OTH",
                iamt = AllOtherItc.GroupedSales.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                camt = AllOtherItc.GroupedSales.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                samt = AllOtherItc.GroupedSales.Select(p => p.TotalSgstValue + p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                csamt = AllOtherItc.GroupedSales.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
            };

            ClsGstResult InwardInterComposition = oGstController.InwardInterComposition(obj);
            decimal _InwardInterComposition = InwardInterComposition.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum();

            ClsGstResult InwardIntraComposition = oGstController.InwardIntraComposition(obj);
            decimal _InwardIntraComposition = InwardIntraComposition.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum();

            ClsGstResult InwardInterNonGst = oGstController.InwardInterNonGst(obj);
            decimal _InwardInterNonGst = InwardInterNonGst.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum();

            ClsGstResult InwardIntraNonGst = oGstController.InwardIntraNonGst(obj);
            decimal _InwardIntraNonGst = InwardIntraNonGst.GroupedSales.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum();

            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting
                    .Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId)
                    .Select(a => a.BusinessRegistrationNo)
                    .FirstOrDefault(),
                ret_period = "03" + obj.ToDate.Year.ToString(),
                sup_details = new
                {
                    osup_det = osup_det,
                    osup_zero = osup_zero,
                    osup_nil_exmp = osup_nil_exmp,
                    isup_rev = isup_rev,
                    osup_nongst = osup_nongst
                },
                eco_dtls = new
                {
                    eco_sup = new { txval = 0, iamt = 0, camt = 0, samt = 0, csamt = 0 },
                    eco_reg_sup = new { txval = 0 }
                },
                itc_elg = new
                {
                    itc_avl = new[]
                    {
                        IMPG,
                        IMPS,
                        ISRC,
                        new { ty = "ISD", iamt = Convert.ToDecimal(0), camt = Convert.ToDecimal(0), samt = Convert.ToDecimal(0), csamt = Convert.ToDecimal(0) },
                        OTH
                    },
                    itc_rev = new[]
                    {
                        new { ty = "RUL", iamt = 0, camt = 0, samt = 0, csamt = 0 },
                        new { ty = "OTH", iamt = 0, camt = 0, samt = 0, csamt = 0 }
                    },
                                itc_net = new { iamt = 0, camt = 0, samt = 0, csamt = 0 },
                                itc_inelg = new[]
                    {
                        new { ty = "RUL", iamt = 0, camt = 0, samt = 0, csamt = 0 },
                        new { ty = "OTH", iamt = 0, camt = 0, samt = 0, csamt = 0 }
                    }
                },
                inward_sup = new
                {
                    isup_details = new[]
                    {
                        new { ty = "GST", inter = _InwardInterComposition, intra = _InwardIntraComposition },
                        new { ty = "NONGST", inter = _InwardInterNonGst, intra = _InwardIntraNonGst }
                    }
                },
                intr_ltfee = new
                {
                    intr_details = new { iamt = 0, camt = 0, samt = 0, csamt = 0 },
                    ltfee_details = new { } // empty object
                },
                inter_sup = new
                {
                    unreg_details = unreg_details,
                    comp_details = comp_details,
                    uin_details = new object[] { }
                }
            };

            return await Task.FromResult(Ok(json));
        }

        #endregion

        #region CMP-08
        public async Task<IHttpActionResult> OutwardIncExempt(ClsSalesVm obj)
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

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxId == oConnectionContext.DbClsTaxSetting.Where(b => b.CompanyId == obj.CompanyId && b.TaxSettingId == obj.TaxSettingId).Select(b => b.CompositionSchemeTaxId).FirstOrDefault()
            ).Select(a => a.TaxPercent).FirstOrDefault();

            var AllSales = (from b in oConnectionContext.DbClsSales
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
            && b.Status != "Draft"
                            select new
                            {
                                SalesId = b.SalesId,
                                SalesDate = b.SalesDate,
                                InvoiceNo = b.InvoiceNo,
                                SalesType = b.SalesType,
                                GrandTotal = b.GrandTotal,
                                TaxPercent = TaxPercent,
                                TotalCgstValue = ((TaxPercent / 100) * b.GrandTotal) / 2,
                                TotalSgstValue = ((TaxPercent / 100) * b.GrandTotal) / 2
                            }).ToList();

            var CreditNotes = (from b in oConnectionContext.DbClsSalesReturn
                               join f in oConnectionContext.DbClsSales
                               on b.SalesId equals f.SalesId
                               where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
               && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
               l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
           && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
               DbFunctions.TruncateTime(b.Date) <= obj.ToDate
               && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false
                               select new
                               {
                                   SalesId = b.SalesId,
                                   SalesDate = b.Date,
                                   InvoiceNo = b.InvoiceNo,
                                   SalesType = "Credit Note",
                                   GrandTotal = -b.GrandTotal,
                                   TaxPercent = TaxPercent,
                                   TotalCgstValue = -((TaxPercent / 100) * b.GrandTotal) / 2,
                                   TotalSgstValue = -((TaxPercent / 100) * b.GrandTotal) / 2
                               }).ToList();

            //var AdvancePayments = (from b in oConnectionContext.DbClsCustomerPayment
            //                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
            //          && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            //          l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            //          && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
            //          DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
            //          && b.Type == "Customer Payment" && b.ParentId == 0 && b.AmountRemaining > 0
            //                       select new
            //                       {
            //                           SalesId = b.CustomerPaymentId,
            //                           SalesDate = b.PaymentDate,
            //                           InvoiceNo = b.ReferenceNo,
            //                           SalesType = b.Type,
            //                           GrandTotal = b.AmountRemaining,
            //                           TaxPercent = TaxPercent,
            //                           TotalCgstValue = ((TaxPercent / 100) * b.AmountRemaining) / 2,
            //                           TotalSgstValue = ((TaxPercent / 100) * b.AmountRemaining) / 2
            //                       }).ToList();

            var det = AllSales.Union(CreditNotes);//.Union(AdvancePayments);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TaxPercent = TaxPercent,
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InwardRevChargeIncImpOfServices(ClsSalesVm obj)
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

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxId == oConnectionContext.DbClsTaxSetting.Where(b => b.CompanyId == obj.CompanyId && b.TaxSettingId == obj.TaxSettingId).Select(b => b.CompositionSchemeTaxId).FirstOrDefault()
            ).Select(a => a.TaxPercent).FirstOrDefault();

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
        && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
        && a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var PurchaseBill = (from b in oConnectionContext.DbClsPurchase
                                where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
                && b.Status != "Draft" && b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 1
                                select new
                                {
                                    SalesDate = b.PurchaseDate,
                                    InvoiceNo = b.ReferenceNo,
                                    GrandTotal = b.GrandTotal,
                                    SalesType = "Purchase Bill",
                                    SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                                    where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                    && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                    select new
                                                    {
                                                        AmountExcTax = c.AmountExcTax,
                                                        TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                    select new
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



            var groupedPurchaseBill = PurchaseBill.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                SalesType = a.SalesType,
                GrandTotal = a.GrandTotal,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var Overseas = (from b in oConnectionContext.DbClsPurchase
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
            && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)"
            && b.IsReverseCharge == 1
                            select new
                            {
                                SalesDate = b.PurchaseDate,
                                InvoiceNo = b.ReferenceNo,
                                GrandTotal = b.GrandTotal,
                                SalesType = "Purchase Bill",
                                SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                                join d in oConnectionContext.DbClsItem
                                                on c.ItemId equals d.ItemId
                                                where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                                && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                && d.ItemType.ToLower() == "service"
                                                select new
                                                {
                                                    AmountExcTax = c.AmountExcTax,
                                                    TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                select new
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

            var groupedOverseas = Overseas.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                SalesType = a.SalesType,
                GrandTotal = a.GrandTotal,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var Expenses = (from b in oConnectionContext.DbClsExpense
                            where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
            && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
            DbFunctions.TruncateTime(b.Date) <= obj.ToDate && b.IsReverseCharge == 1
                            select new
                            {
                                SalesDate = b.Date,
                                InvoiceNo = b.ReferenceNo,
                                GrandTotal = b.GrandTotal,
                                SalesType = "Expense",
                                SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                                where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                                && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                select new
                                                {
                                                    AmountExcTax = c.AmountExcTax,
                                                    TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                select new
                                                                {
                                                                    TaxType = x.TaxType,
                                                                    TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                                 join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                                 on y.TaxId equals z.TaxId
                                                                                 where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                                                                 && z.ExpenseId == b.ExpenseId
                                                                                 && z.ExpensePaymentId == c.ExpensePaymentId
                                                                                 select z.TaxAmount
                                                                                ).DefaultIfEmpty().Sum()
                                                                }).ToList()
                                                }).ToList()
                            }).ToList();

            var groupedExpenses = Expenses.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                SalesType = a.SalesType,
                GrandTotal = a.GrandTotal,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var DebitNotes = (from b in oConnectionContext.DbClsPurchaseReturn
                              join f in oConnectionContext.DbClsPurchase
                              on b.PurchaseId equals f.PurchaseId
                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
              && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
              l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
          && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
              DbFunctions.TruncateTime(b.Date) <= obj.ToDate
              && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false && b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 1
                              select new
                              {
                                  SalesDate = b.Date,
                                  InvoiceNo = b.InvoiceNo,
                                  GrandTotal = b.GrandTotal,
                                  SalesType = "Debit Note",
                                  SalesDetails = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                                                  where c.PurchaseReturnId == b.PurchaseReturnId && c.IsDeleted == false && c.IsActive == true
                                                  && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                  select new
                                                  {
                                                      AmountExcTax = c.AmountExcTax,
                                                      TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                  select new
                                                                  {
                                                                      TaxType = x.TaxType,
                                                                      TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                                   join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                                                   on y.TaxId equals z.TaxId
                                                                                   where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseReturnId == b.PurchaseReturnId
                                                                                   && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                                                   select z.TaxAmount
                                                                                  ).DefaultIfEmpty().Sum()
                                                                  }).ToList()
                                                  }).ToList()
                              }).ToList();

            var groupedDebitNotes = DebitNotes.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                SalesType = a.SalesType,
                GrandTotal = -a.GrandTotal,
                AmountExcTax = -a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var DebitNotesOverseas = (from b in oConnectionContext.DbClsPurchaseReturn
                                      join f in oConnectionContext.DbClsPurchase
                                      on b.PurchaseId equals f.PurchaseId
                                      where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                      && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                      l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                  && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                      && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 1
                                      select new
                                      {
                                          SalesDate = b.Date,
                                          InvoiceNo = b.InvoiceNo,
                                          GrandTotal = b.GrandTotal,
                                          SalesType = "Debit Note",
                                          SalesDetails = (from c in oConnectionContext.DbClsPurchaseReturnDetails
                                                          join d in oConnectionContext.DbClsItem
                                                          on c.ItemId equals d.ItemId
                                                          where c.PurchaseReturnId == b.PurchaseReturnId && c.IsDeleted == false && c.IsActive == true
                                                          && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                                          && d.ItemType.ToLower() == "service"
                                                          select new
                                                          {
                                                              AmountExcTax = c.AmountExcTax,
                                                              TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                                          select new
                                                                          {
                                                                              TaxType = x.TaxType,
                                                                              TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                                           join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                                                           on y.TaxId equals z.TaxId
                                                                                           where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseReturnId == b.PurchaseReturnId
                                                                                           && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                                                           select z.TaxAmount
                                                                                          ).DefaultIfEmpty().Sum()
                                                                              //TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                              //             join z in oConnectionContext.DbClsPurchaseReturnTaxJournal
                                                                              //             on y.TaxId equals z.TaxId
                                                                              //             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseReturnId == b.PurchaseReturnId
                                                                              //             && z.PurchaseReturnDetailsId == c.PurchaseReturnDetailsId
                                                                              //             select z.TaxAmount
                                                                              //            ).DefaultIfEmpty().Sum()
                                                                          }).ToList()
                                                          }).ToList()
                                      }).ToList();

            var groupedDebitNotesOverseas = DebitNotesOverseas.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                SalesType = a.SalesType,
                GrandTotal = -a.GrandTotal,
                AmountExcTax = -a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = -a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            var AdvancePayments = (from b in oConnectionContext.DbClsSupplierPayment
                                   where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                      && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                      l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                      && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
                      DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
                      && b.Type == "Supplier Payment" && b.ParentId == 0 && b.IsDeleted == false && b.PurchaseReturnId == 0
                      && b.IsReverseCharge == 1
                                   select new ClsSalesVm
                                   {
                                       SalesDate = b.PaymentDate,
                                       InvoiceNo = b.ReferenceNo,
                                       SalesType = b.Type,
                                       GrandTotal = b.Amount - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                             b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                             && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                             DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                    ).Select(c => c.Amount).DefaultIfEmpty().Sum(),
                                       AmountExcTax = b.AmountExcTax - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
                                             b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
                                             && DbFunctions.TruncateTime(c.PaymentDate) >= obj.FromDate &&
                                             DbFunctions.TruncateTime(c.PaymentDate) <= obj.ToDate
                                                    ).Select(c => c.AmountExcTax).DefaultIfEmpty().Sum(),
                                       TaxId = b.TaxId,
                                       //TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       //            select new
                                       //            {
                                       //                TaxType = x.TaxType,
                                       //                TaxAmount = (from y in oConnectionContext.DbClsTax
                                       //                             join z in oConnectionContext.DbClsSupplierPaymentTaxJournal
                                       //                             on y.TaxId equals z.TaxId
                                       //                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId
                                       //                             && z.SupplierPaymentId == b.SupplierPaymentId
                                       //                             select z.TaxAmount
                                       //                            ).DefaultIfEmpty().Sum()
                                       //            }).ToList()
                                   }).ToList();

            foreach (var item in AdvancePayments)
            {
                var Tax = new
                {
                    IsTaxGroup = oConnectionContext.DbClsTax.Where(b => b.TaxId == item.TaxId).Select(b => b.IsTaxGroup).FirstOrDefault(),
                    TaxId = item.TaxId,
                    AmountExcTax = item.AmountExcTax
                };

                List<ClsTaxVm> oClsTaxVm = new List<ClsTaxVm>();
                decimal AmountExcTax = Tax.AmountExcTax;
                var taxs = Tax.IsTaxGroup == false ? oConnectionContext.DbClsTax.Where(a => a.TaxId == Tax.TaxId).Select(a => new
                {
                    TaxType = (from x in oConnectionContext.DbClsTaxType where x.TaxTypeId == a.TaxTypeId select x.TaxType).FirstOrDefault(),
                    a.TaxId,
                    a.Tax,
                    a.TaxPercent,
                }).ToList() : (from a in oConnectionContext.DbClsTaxMap
                               join b in oConnectionContext.DbClsTax
                               on a.SubTaxId equals b.TaxId
                               where a.TaxId == Tax.TaxId
                               select new
                               {
                                   TaxType = (from x in oConnectionContext.DbClsTaxType where x.TaxTypeId == b.TaxTypeId select x.TaxType).FirstOrDefault(),
                                   TaxId = a.SubTaxId,
                                   Tax = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.Tax).FirstOrDefault(),
                                   TaxPercent = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.SubTaxId).Select(b => b.TaxPercent).FirstOrDefault(),
                               }).ToList();

                foreach (var tax in taxs)
                {
                    oClsTaxVm.Add(new ClsTaxVm
                    {
                        TaxType = tax.TaxType,
                        TaxId = tax.TaxId,
                        Tax = tax.Tax,
                        TaxPercent = tax.TaxPercent,
                        TaxAmount = (tax.TaxPercent / 100) * AmountExcTax
                    });
                }

                var finalTaxs = oClsTaxVm.GroupBy(p => p.Tax,
                         (k, c) => new ClsTaxTypeVm
                         {
                             TaxType = c.Select(cs => cs.TaxType).FirstOrDefault(),
                             //TaxId = c.Select(cs => cs.TaxId).FirstOrDefault(),
                             //Tax = c.Select(cs => cs.Tax).FirstOrDefault(),
                             //TaxPercent = c.Select(cs => cs.TaxPercent).FirstOrDefault(),
                             TaxAmount = c.Select(cs => cs.TaxAmount).DefaultIfEmpty().Sum()
                         }
                        ).ToList();

                item.TaxTypes = finalTaxs;
            }

            var groupedAdvancePayments = AdvancePayments.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                SalesType = a.SalesType,
                GrandTotal = a.GrandTotal,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
            }).ToList();

            //var groupedAdvancePayments = (from b in oConnectionContext.DbClsSupplierPayment
            //                              where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
            //                 && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
            //                 l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
            //                 && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
            //                 DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
            //                 && b.Type == "Supplier Payment" && b.ParentId == 0 && b.IsDeleted == false && b.PurchaseReturnId == 0
            //                 && b.IsReverseCharge == 1
            //                              select new ClsSalesVm
            //                              {
            //                                  SalesDate = b.PaymentDate,
            //                                  InvoiceNo = b.ReferenceNo,
            //                                  SalesType = b.Type,
            //                                  //GrandTotal = b.AmountRemaining,
            //                                  GrandTotal = b.Amount - oConnectionContext.DbClsSupplierPayment.Where(c => c.ParentId ==
            //                                        b.SupplierPaymentId && c.IsDeleted == false && c.IsCancelled == false
            //                                        && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
            //                                        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
            //                                        ).Select(c => c.Amount).DefaultIfEmpty().Sum(),
            //                                  AmountExcTax = (decimal)0,
            //                                  TaxPercent = TaxPercent,
            //                                  TotalCgstValue = ((TaxPercent / 100) * b.AmountRemaining) / 2,
            //                                  TotalSgstValue = ((TaxPercent / 100) * b.AmountRemaining) / 2,
            //                                  TotalUtgstValue = (decimal)0,
            //                                  TotalIgstValue = (decimal)0,
            //                                  TotalCessValue = (decimal)0
            //                              }).ToList();

            var det = groupedPurchaseBill.Union(groupedOverseas)
                .Union(groupedExpenses)
                .Union(groupedDebitNotes)
                .Union(groupedDebitNotesOverseas)
                .Union(groupedAdvancePayments)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }
        #endregion

        #region Gstr 4

        public async Task<IHttpActionResult> Gstr4Sec4A(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4A(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4ADetails(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4A(obj);

            var det1 = det
                .GroupBy(pb => new { pb.StateCode, pb.BusinessRegistrationNo, pb.DestinationOfSupply, pb.SupplyType })
                .Select(g => new
                {
                    g.Key.StateCode,
                    g.Key.BusinessRegistrationNo,
                    g.Key.DestinationOfSupply,
                    g.Key.SupplyType,
                    SalesDetails = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                        .Select(tg => new
                        {
                            TaxPercent = tg.Key,
                            AmountExcTax = tg.Sum(pb => pb.AmountExcTax), // Summing AmountExcTax
                            IGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                            CGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                            SGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST").Sum(t => t.TaxAmount)),
                            CESS = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                        }).ToList()
                }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det1.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4B(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4B(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4BDetails(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4B(obj);

            var det1 = det
                .GroupBy(pb => new { pb.StateCode, pb.BusinessRegistrationNo, pb.DestinationOfSupply, pb.SupplyType })
                .Select(g => new
                {
                    g.Key.StateCode,
                    g.Key.BusinessRegistrationNo,
                    g.Key.DestinationOfSupply,
                    g.Key.SupplyType,
                    SalesDetails = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                        .Select(tg => new
                        {
                            TaxPercent = tg.Key,
                            AmountExcTax = tg.Sum(pb => pb.AmountExcTax), // Summing AmountExcTax
                            IGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                            CGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                            SGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST").Sum(t => t.TaxAmount)),
                            CESS = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                        }).ToList()
                }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det1.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4C(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4C(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4CDetails(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4C(obj);

            //var distinctInvoice = det.Select(a => new
            //{
            //    a.PanNo,
            //    a.BusinessTradeName,
            //    a.IsReverseCharge,
            //    a.StateCode,
            //    a.InvoiceNo,
            //    a.DestinationOfSupply,
            //    a.SupplyType
            //}).Distinct().ToList();

            //var det1 = distinctInvoice.Select(a => new
            //{
            //    a.PanNo,
            //    a.BusinessTradeName,
            //    a.IsReverseCharge,
            //    a.StateCode,
            //    a.InvoiceNo,
            //    a.DestinationOfSupply,
            //    a.SupplyType,
            //    SalesDetails = det.Where(b => b.InvoiceNo == a.InvoiceNo).Select(b => new
            //    {
            //        b.TaxPercent,
            //        b.AmountExcTax,
            //        TaxTypes = b.TaxTypes
            //    })
            //}).ToList();

            var det1 = det
                .GroupBy(pb => new { pb.StateCode, pb.PanNo, pb.DestinationOfSupply, pb.SupplyType })
                .Select(g => new
                {
                    IsReverseCharge = g.Select(pb => pb.IsReverseCharge).FirstOrDefault(),
                    g.Key.StateCode,
                    g.Key.PanNo,
                    g.Key.DestinationOfSupply,
                    g.Key.SupplyType,
                    SalesDetails = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                        .Select(tg => new
                        {
                            TaxPercent = tg.Key,
                            AmountExcTax = tg.Sum(pb => pb.AmountExcTax), // Summing AmountExcTax
                            IGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                            CGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                            SGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST").Sum(t => t.TaxAmount)),
                            CESS = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                        }).ToList()
                }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det1.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4D(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4D(obj);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.Select(a => a.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr4Sec4DDetails(ClsSalesVm obj)
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

            var det = oCommonController.Gstr4Sec4D(obj);

            //var distinctInvoice = det.Select(a => new
            //{
            //    a.StateCode,
            //    a.DestinationOfSupply
            //}).Distinct().ToList();

            //var det1 = distinctInvoice.Select(a => new
            //{
            //    a.StateCode,
            //    a.DestinationOfSupply,
            //    SalesDetails = det.Where(b => b.DestinationOfSupply == a.DestinationOfSupply).Select(b => new
            //    {
            //        b.TaxPercent,
            //        b.AmountExcTax,
            //        TaxTypes = b.TaxTypes
            //    })
            //}).ToList();

            var det1 = det
               .GroupBy(pb => new { pb.StateCode, pb.DestinationOfSupply })
               .Select(g => new
               {
                   g.Key.StateCode,
                   g.Key.DestinationOfSupply,
                   SalesDetails = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                       .Select(tg => new
                       {
                           TaxPercent = tg.Key,
                           AmountExcTax = tg.Sum(pb => pb.AmountExcTax), // Summing AmountExcTax
                           IGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                           CGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                           SGST = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST").Sum(t => t.TaxAmount)),
                           CESS = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                       }).ToList()
               }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det1.OrderByDescending(a => a.StateCode).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> GenerateGstr4Json(ClsSalesVm obj)
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

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            decimal TaxPercent = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId &&
            a.TaxId == oConnectionContext.DbClsTaxSetting.Where(b => b.CompanyId == obj.CompanyId && b.TaxSettingId == obj.TaxSettingId).Select(b => b.CompositionSchemeTaxId).FirstOrDefault()
            ).Select(a => a.TaxPercent).FirstOrDefault();

            var Gstr4Sec4ADetails = oCommonController.Gstr4Sec4A(obj);

            var Gstr4Sec4A = Gstr4Sec4ADetails
                .GroupBy(pb => new { pb.StateCode, pb.BusinessRegistrationNo, pb.DestinationOfSupply, pb.SupplyType })
                .Select(g => new
                {
                    ctin = g.Key.BusinessRegistrationNo,
                    pos = g.Key.StateCode,
                    itms = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                        .Select(tg => new
                        {
                            txval = tg.Sum(pb => pb.AmountExcTax),
                            rt = tg.Key,
                            iamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                            camt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                            samt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount)),
                            csamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                        }).ToList()
                }).ToList();

            var Gstr4Sec4BDetails = oCommonController.Gstr4Sec4B(obj);

            var Gstr4Sec4B = Gstr4Sec4BDetails
                .GroupBy(pb => new { pb.StateCode, pb.BusinessRegistrationNo, pb.DestinationOfSupply, pb.SupplyType })
                .Select(g => new
                {
                    ctin = g.Key.BusinessRegistrationNo,
                    pos = g.Key.StateCode,
                    itms = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                        .Select(tg => new
                        {
                            txval = tg.Sum(pb => pb.AmountExcTax),
                            rt = tg.Key,
                            iamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                            camt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                            samt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount)),
                            csamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                        }).ToList()
                }).ToList();

            var Gstr4Sec4CDetails = oCommonController.Gstr4Sec4C(obj);

            var Gstr4Sec4C = Gstr4Sec4CDetails
                .GroupBy(pb => new { pb.StateCode, pb.PanNo, pb.DestinationOfSupply, pb.SupplyType })
                .Select(g => new
                {
                    cpan = g.Key.PanNo,
                    rchrg = g.Select(pb => pb.IsReverseCharge).FirstOrDefault(),
                    pos = g.Key.StateCode,
                    sply_ty = g.Key.SupplyType == "Intra-State" ? "INTRA" : "INTER",
                    itms = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                        .Select(tg => new
                        {
                            txval = tg.Sum(pb => pb.AmountExcTax),
                            rt = tg.Key,
                            iamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                            camt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CGST").Sum(t => t.TaxAmount)),
                            samt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "SGST" || t.TaxType == "UTGST").Sum(t => t.TaxAmount)),
                            csamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                        }).ToList()
                }).ToList();

            var Gstr4Sec4DDetails = oCommonController.Gstr4Sec4D(obj);

            var Gstr4Sec4D = Gstr4Sec4DDetails
               .GroupBy(pb => new { pb.StateCode, pb.DestinationOfSupply })
               .Select(g => new
               {
                   pos = g.Key.StateCode,
                   itms = g.GroupBy(pb => pb.TaxPercent) // Grouping by TaxPercent
                       .Select(tg => new
                       {
                           txval = tg.Sum(pb => pb.AmountExcTax),
                           rt = tg.Key,
                           iamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "IGST").Sum(t => t.TaxAmount)),
                           csamt = tg.Sum(pb => pb.TaxTypes.Where(t => t.TaxType == "CESS").Sum(t => t.TaxAmount))
                       }).ToList()
               }).ToList();

            var Gstr4Sec6AllSales = (from b in oConnectionContext.DbClsSales
                                     where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                     && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                     l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                 && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
                     DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
                     && b.Status != "Draft"
                                     select new
                                     {
                                         SalesId = b.SalesId,
                                         SalesDate = b.SalesDate,
                                         InvoiceNo = b.InvoiceNo,
                                         SalesType = b.SalesType,
                                         GrandTotal = b.GrandTotal,
                                         TaxPercent = TaxPercent,
                                         TotalCgstValue = ((TaxPercent / 100) * b.GrandTotal) / 2,
                                         TotalSgstValue = ((TaxPercent / 100) * b.GrandTotal) / 2
                                     }).ToList();

            var Gstr4Sec6CreditNotes = (from b in oConnectionContext.DbClsSalesReturn
                                        join f in oConnectionContext.DbClsSales
                                        on b.SalesId equals f.SalesId
                                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
                        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
                        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                    && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
                        && b.Status != "Draft" && f.IsDeleted == false && f.IsCancelled == false
                                        select new
                                        {
                                            SalesId = b.SalesId,
                                            SalesDate = b.Date,
                                            InvoiceNo = b.InvoiceNo,
                                            SalesType = "Credit Note",
                                            GrandTotal = -b.GrandTotal,
                                            TaxPercent = TaxPercent,
                                            TotalCgstValue = -((TaxPercent / 100) * b.GrandTotal) / 2,
                                            TotalSgstValue = -((TaxPercent / 100) * b.GrandTotal) / 2
                                        }).ToList();

            var Gstr4Sec6 = Gstr4Sec6AllSales.Union(Gstr4Sec6CreditNotes);

            var outsupply = new[]
                {
                    new { rt = 0, txval = TaxPercent == 0 ? Gstr4Sec6.Select(a => a.GrandTotal).DefaultIfEmpty().Sum() : 0 },
                    new { rt = 1, txval = TaxPercent == 1 ? Gstr4Sec6.Select(a => a.GrandTotal).DefaultIfEmpty().Sum() : 0 },
                    new { rt = 2, txval = TaxPercent == 2 ? Gstr4Sec6.Select(a => a.GrandTotal).DefaultIfEmpty().Sum() : 0 },
                    new { rt = 5, txval = TaxPercent == 5 ? Gstr4Sec6.Select(a => a.GrandTotal).DefaultIfEmpty().Sum() : 0 },
                    new { rt = 6, txval = TaxPercent == 6 ? Gstr4Sec6.Select(a => a.GrandTotal).DefaultIfEmpty().Sum() : 0 }
                };


            var json = new
            {
                gstin = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false && a.TaxSettingId == obj.TaxSettingId).Select(a => a.BusinessRegistrationNo).FirstOrDefault(),
                fp = "03" + obj.ToDate.Year.ToString(),
                version = "2.3",
                b2bor = Gstr4Sec4A,
                b2br = Gstr4Sec4B,
                b2bur = Gstr4Sec4C,
                imps = Gstr4Sec4D,
                outsupply = outsupply
            };

            return await Task.FromResult(Ok(json));
        }

        #endregion

        #region Gstr 9

        public async Task<IHttpActionResult> Gstr9_4A(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer")
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4B(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor")
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4C(ClsSalesVm obj)
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
        && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.PayTaxForExport == 1
        && !oConnectionContext.DbClsShippingBill.Any(r => r.SalesId == b.SalesId)
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            SalesDate = b.SalesDate,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            PortCode = "",
                            ShippingBillNo = "",
                            ShippingBillDate = DateTime.MinValue,
                            GrandTotal = b.GrandTotal,
                            SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                            where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsSales
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.PayTaxForExport == 1
        && oConnectionContext.DbClsShippingBill.Any(r => r.SalesId == b.SalesId)
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            SalesDate = b.SalesDate,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            PortCode = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.PortCode).FirstOrDefault(),
                            ShippingBillNo = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ShippingBillNo).FirstOrDefault(),
                            ShippingBillDate = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ShippingBillDate).FirstOrDefault(),
                            GrandTotal = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.GrandTotal).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                            where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ShippingBillDate = a.ShippingBillDate,
                ShippingBillNo = a.ShippingBillNo,
                PortCode = a.PortCode,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4D(ClsSalesVm obj)
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
        && b.Status != "Draft" && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1
        && !oConnectionContext.DbClsShippingBill.Any(r => r.SalesId == b.SalesId)
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            SalesDate = b.SalesDate,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            PortCode = "",
                            ShippingBillNo = "",
                            ShippingBillDate = DateTime.MinValue,
                            GrandTotal = b.GrandTotal,
                            SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                            where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsSales
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 1
        && oConnectionContext.DbClsShippingBill.Any(r => r.SalesId == b.SalesId)
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            SalesDate = b.SalesDate,
                            InvoiceNo = b.InvoiceNo,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            PortCode = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.PortCode).FirstOrDefault(),
                            ShippingBillNo = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ShippingBillNo).FirstOrDefault(),
                            ShippingBillDate = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.ShippingBillDate).FirstOrDefault(),
                            GrandTotal = oConnectionContext.DbClsShippingBill.Where(c => c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true).Select(c => c.GrandTotal).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                            where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ShippingBillDate = a.ShippingBillDate,
                ShippingBillNo = a.ShippingBillNo,
                PortCode = a.PortCode,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4F(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsCustomerPayment
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PaymentDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PaymentDate) <= obj.ToDate
        && b.Type == "Customer Payment" && b.ParentId == 0 //&& b.TaxAmount > 0
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.CustomerPaymentId,
                           SalesDate = b.PaymentDate,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.AmountRemaining,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           AmountExcTax = (100 * (b.AmountRemaining) / (100 +
                                          oConnectionContext.DbClsTax.Where(a => a.TaxId == b.TaxId).Select(a => a.TaxPercent).FirstOrDefault())),
                           b.TaxId,
                           TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == b.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsTaxTypeVm
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsCustomerPaymentTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId &&
                                                        z.CustomerPaymentId == b.CustomerPaymentId
                                                        select ((y.TaxPercent / 100) * (100 * (b.AmountRemaining) / (100 +
                                          oConnectionContext.DbClsTax.Where(a => a.TaxId == b.TaxId).Select(a => a.TaxPercent).FirstOrDefault())))
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4G(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate && b.IsReverseCharge == 1
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.PurchaseId,
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4I(ClsSalesVm obj)
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

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det1 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor")
         && b.TotalTaxAmount > 0 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && c.PayTaxForExport == 1 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det3 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && c.PayTaxForExport == 1 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det = det1.Union(det2).Union(det3).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ParentInvoiceNo = a.ParentInvoiceNo,
                ParentSalesDate = a.ParentSalesDate,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                Reason = a.Reason,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_4J(ClsSalesVm obj)
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
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor")
         && b.TotalTaxAmount > 0 && c.IsDeleted == false && c.IsCancelled == false && b.SalesType == "Debit Note"
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Debit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det2 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && c.PayTaxForExport == 1 && c.IsDeleted == false && c.IsCancelled == false && b.SalesType == "Debit Note"
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Debit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det3 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && c.PayTaxForExport == 1 && c.IsDeleted == false && c.IsCancelled == false
        && b.SalesType == "Debit Note"
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Debit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det = det1.Union(det2).Union(det3).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ParentInvoiceNo = a.ParentInvoiceNo,
                ParentSalesDate = a.ParentSalesDate,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                Reason = a.Reason,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5A(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.PayTaxForExport == 2
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           GrandTotal = b.GrandTotal,
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5B(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && b.PayTaxForExport == 2
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           GrandTotal = b.GrandTotal,
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5C(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate && b.IsReverseCharge == 1
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5D(ClsSalesVm obj)
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

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long ExemptedId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
        && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId == ExemptedId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5E(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5F(ClsSalesVm obj)
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

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            long NonGstId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
        && a.Tax == "Non-GST Supply").Select(a => a.TaxId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsSales
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           SalesDate = b.SalesDate,
                           InvoiceNo = b.InvoiceNo,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsSalesDetails
                                           where c.SalesId == b.SalesId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId == NonGstId)
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5H(ClsSalesVm obj)
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

            var det1 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && c.PayTaxForExport == 2 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det2 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && c.PayTaxForExport == 2 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det3 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && c.IsReverseCharge == 1
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det4 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId
                            && d.IsDeleted == false && d.IsActive == true && d.TaxId == ExemptedId).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det5 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId
                            && d.IsDeleted == false && d.IsActive == true && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det6 = (from b in oConnectionContext.DbClsSalesReturn
                        join c in oConnectionContext.DbClsSales
                        on b.SalesId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.Date,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId
                            && d.IsDeleted == false && d.IsActive == true && (c.TaxId == NonGstId)).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det = det1.Union(det2).Union(det3).Union(det4).Union(det5).Union(det6).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ParentInvoiceNo = a.ParentInvoiceNo,
                ParentSalesDate = a.ParentSalesDate,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                Reason = a.Reason,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_5I(ClsSalesVm obj)
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

            var det1 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && c.PayTaxForExport == 2 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det2 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && (b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && c.PayTaxForExport == 2 && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det3 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft" && c.IsReverseCharge == 1
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply")
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                            && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det4 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId
                            && d.IsDeleted == false && d.IsActive == true && d.TaxId == ExemptedId).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det5 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId
                            && d.IsDeleted == false && d.IsActive == true && (c.TaxId == NilratedGstId && c.TaxId == NilratedIgstId)).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det6 = (from b in oConnectionContext.DbClsSales
                        join c in oConnectionContext.DbClsSales
                        on b.ParentId equals c.SalesId
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
        && b.Status != "Draft"
        && c.IsDeleted == false && c.IsCancelled == false
                        select new
                        {
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                            InvoiceNo = b.InvoiceNo,
                            SalesDate = b.SalesDate,
                            ParentInvoiceNo = c.InvoiceNo,
                            ParentSalesDate = c.SalesDate,
                            SalesType = "Credit Note",
                            GrandTotal = b.GrandTotal,
                            Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.SalesId,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId
                            && d.IsDeleted == false && d.IsActive == true && (c.TaxId == NonGstId)).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                            TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                        select new ClsTaxTypeVm
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

            var det = det1.Union(det2).Union(det3).Union(det4).Union(det5).Union(det6).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ParentInvoiceNo = a.ParentInvoiceNo,
                ParentSalesDate = a.ParentSalesDate,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                Reason = a.Reason,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6BInputs(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer") && b.IsReverseCharge == 2
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Product"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer") && b.IsReverseCharge == 2
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            join d in oConnectionContext.DbClsAccount
                                            on c.AccountId equals d.AccountId
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId != FixedAssetId
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6BCapitalGoods(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsExpense
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.GstTreatment != "Supply to SEZ Unit (Zero-Rated Supply)" && b.GstTreatment != "Supply by SEZ Developer") && b.IsReverseCharge == 2
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.ExpenseId,
                           SalesDate = b.Date,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                           join d in oConnectionContext.DbClsAccount
                                           on c.AccountId equals d.AccountId
                                           where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId == FixedAssetId
                                           && c.ITCType == "Eligible For ITC"
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                            && z.ExpensePaymentId == c.ExpensePaymentId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6BInputServices(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 2
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 2
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6CInputs(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Product"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            join d in oConnectionContext.DbClsAccount
                                            on c.AccountId equals d.AccountId
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId != FixedAssetId
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6CCapitalGoods(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsExpense
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.IsReverseCharge == 1
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.ExpenseId,
                           SalesDate = b.Date,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                           join d in oConnectionContext.DbClsAccount
                                           on c.AccountId equals d.AccountId
                                           where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId == FixedAssetId
                                           && c.ITCType == "Eligible For ITC"
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                            && z.ExpensePaymentId == c.ExpensePaymentId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6CInputServices(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Taxable Supply to Unregistered Person" || b.GstTreatment == "Taxable Supply to Consumer") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6DInputs(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Product"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            join d in oConnectionContext.DbClsAccount
                                            on c.AccountId equals d.AccountId
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId != FixedAssetId
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6DCapitalGoods(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsExpense
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor") && b.IsReverseCharge == 1
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.ExpenseId,
                           SalesDate = b.Date,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                           join d in oConnectionContext.DbClsAccount
                                           on c.AccountId equals d.AccountId
                                           where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId == FixedAssetId
                                           && c.ITCType == "Eligible For ITC"
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                            && z.ExpensePaymentId == c.ExpensePaymentId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6DInputServices(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Taxable Supply (Registered)" || b.GstTreatment == "Composition Taxable Supply"
        || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Deemed Export" || b.GstTreatment == "Supply by SEZ Developer"
        || b.GstTreatment == "Tax Deductor") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6EInputs(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer")
        && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Product"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            join d in oConnectionContext.DbClsAccount
                                            on c.AccountId equals d.AccountId
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId != FixedAssetId
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6ECapitalGoods(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det = (from b in oConnectionContext.DbClsExpense
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && (b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" || b.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || b.GstTreatment == "Supply by SEZ Developer") && b.IsReverseCharge == 1
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.ExpenseId,
                           SalesDate = b.Date,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                           join d in oConnectionContext.DbClsAccount
                                           on c.AccountId equals d.AccountId
                                           where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Product" && d.AccountSubTypeId == FixedAssetId
                                           && c.ITCType == "Eligible For ITC"
                                           select new
                                           {
                                               TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                           select new
                                                           {
                                                               TaxTypeId = x.TaxTypeId,
                                                               TaxType = x.TaxType,
                                                               TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                            join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                            on y.TaxId equals z.TaxId
                                                                            where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                            && z.ExpensePaymentId == c.ExpensePaymentId
                                                                            select z.TaxAmount
                                                                           ).DefaultIfEmpty().Sum()
                                                           }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_6F(ClsSalesVm obj)
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

            long FixedAssetId = oConnectionContext.DbClsAccountSubType.Where(a => a.AccountSubType == "Fixed Asset").Select(a => a.AccountSubTypeId).FirstOrDefault();

            var det1 = (from b in oConnectionContext.DbClsPurchase
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate
        && b.Status != "Draft" && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.PurchaseId,
                            SalesDate = b.PurchaseDate,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                            join d in oConnectionContext.DbClsItem
                                            on c.ItemId equals d.ItemId
                                            where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && d.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
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

            var det2 = (from b in oConnectionContext.DbClsExpense
                        where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && b.GstTreatment == "Export of Goods / Services (Zero-Rated Supply)" && b.IsReverseCharge == 1
                        select new
                        {
                            CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                            StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                            SalesId = b.ExpenseId,
                            SalesDate = b.Date,
                            InvoiceNo = b.ReferenceNo,
                            GrandTotal = b.GrandTotal,
                            PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                            BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                            SalesDetails = (from c in oConnectionContext.DbClsExpensePayment
                                            where c.ExpenseId == b.ExpenseId && c.IsDeleted == false && c.IsActive == true
                                            && (c.TaxId != ExemptedId && c.TaxId != NonGstId) && c.ItemType == "Service"
                                            && c.ITCType == "Eligible For ITC"
                                            select new
                                            {
                                                TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                                AmountExcTax = c.AmountExcTax,
                                                TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                                            select new
                                                            {
                                                                TaxTypeId = x.TaxTypeId,
                                                                TaxType = x.TaxType,
                                                                TaxAmount = (from y in oConnectionContext.DbClsTax
                                                                             join z in oConnectionContext.DbClsExpenseTaxJournal
                                                                             on y.TaxId equals z.TaxId
                                                                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.ExpenseId == b.ExpenseId
                                                                             && z.ExpensePaymentId == c.ExpensePaymentId
                                                                             select z.TaxAmount
                                                                            ).DefaultIfEmpty().Sum()
                                                            }).ToList()
                                            }).ToList()

                        }).ToList();

            var det = det1.Union(det2).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_10(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       join c in oConnectionContext.DbClsSales
                       on b.ParentId equals c.SalesId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && DbFunctions.TruncateTime(c.SalesDate) < obj.FromDate
        && b.Status != "Draft" && b.TotalTaxAmount > 0 && c.IsDeleted == false && c.IsCancelled == false && b.SalesType == "Debit Note"
                       select new
                       {
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           InvoiceNo = b.InvoiceNo,
                           SalesDate = b.SalesDate,
                           ParentInvoiceNo = c.InvoiceNo,
                           ParentSalesDate = c.SalesDate,
                           SalesType = "Debit Note",
                           GrandTotal = b.GrandTotal,
                           Reason = oConnectionContext.DbClsSalesDebitNoteReason.Where(d => d.SalesDebitNoteReasonId == b.SalesDebitNoteReasonId).Select(d => d.SalesDebitNoteReason).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == b.SalesId && d.TaxAmount > 0
                           && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsTaxTypeVm
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ParentInvoiceNo = a.ParentInvoiceNo,
                ParentSalesDate = a.ParentSalesDate,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                Reason = a.Reason,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_11(ClsSalesVm obj)
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

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.TaxSettingId == obj.TaxSettingId).Select(a => a.StateId).FirstOrDefault();

            if (obj.TaxSettingId == 0)
            {
                obj.TaxSettingId = oConnectionContext.DbClsTaxSetting.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true
                && a.IsDeleted == false).OrderBy(a => a.BusinessRegistrationNo).Select(a => a.TaxSettingId).FirstOrDefault();
            }

            var det = (from b in oConnectionContext.DbClsSalesReturn
                       join c in oConnectionContext.DbClsSales
                       on b.SalesId equals c.SalesId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.Date) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.Date) <= obj.ToDate
        && DbFunctions.TruncateTime(c.SalesDate) < obj.FromDate
        && b.Status != "Draft" && b.TotalTaxAmount > 0 && c.IsDeleted == false && c.IsCancelled == false
                       select new
                       {
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           InvoiceNo = b.InvoiceNo,
                           SalesDate = b.Date,
                           ParentInvoiceNo = c.InvoiceNo,
                           ParentSalesDate = c.SalesDate,
                           SalesType = "Credit Note",
                           GrandTotal = b.GrandTotal,
                           Reason = oConnectionContext.DbClsSalesCreditNoteReason.Where(d => d.SalesCreditNoteReasonId == b.SalesCreditNoteReasonId).Select(d => d.SalesCreditNoteReason).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.SalesId,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           AmountExcTax = oConnectionContext.DbClsSalesReturnDetails.Where(d => d.SalesReturnId == b.SalesReturnId && d.TaxAmount > 0
                           && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsTaxTypeVm
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

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                ParentInvoiceNo = a.ParentInvoiceNo,
                ParentSalesDate = a.ParentSalesDate,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                Reason = a.Reason,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_14(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsSales
                       join c in oConnectionContext.DbClsSales
                       on b.ParentId equals c.SalesId
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.SalesDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.SalesDate) <= obj.ToDate
        && DbFunctions.TruncateTime(c.SalesDate) < obj.FromDate
        && b.Status != "Draft" && b.TotalTaxAmount > 0 && c.IsDeleted == false && c.IsCancelled == false && b.SalesType == "Debit Note"
                       select new
                       {
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.CustomerId).Select(d => d.Name).FirstOrDefault(),
                           InvoiceNo = c.InvoiceNo,
                           SalesDate = c.SalesDate,
                           SalesType = c.SalesType,
                           GrandTotal = c.GrandTotal,
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.PlaceOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = c.SalesId,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == c.PlaceOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           AmountExcTax = oConnectionContext.DbClsSalesDetails.Where(d => d.SalesId == c.SalesId && d.TaxAmount > 0
                           && d.IsDeleted == false && d.IsActive == true).Select(d => d.AmountExcTax).DefaultIfEmpty().Sum(),
                           TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                       select new ClsTaxTypeVm
                                       {
                                           TaxTypeId = x.TaxTypeId,
                                           TaxType = x.TaxType,
                                           TaxAmount = (from y in oConnectionContext.DbClsTax
                                                        join z in oConnectionContext.DbClsSalesTaxJournal
                                                        on y.TaxId equals z.TaxId
                                                        where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.SalesId == c.SalesId
                                                        select z.TaxAmount
                                                       ).DefaultIfEmpty().Sum()
                                       }).ToList()
                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.AmountExcTax,
                TotalCgstValue = a.TaxTypes.Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalSgstValue = a.TaxTypes.Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalUtgstValue = a.TaxTypes.Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalIgstValue = a.TaxTypes.Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                TotalCessValue = a.TaxTypes.Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = groupedResult.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = groupedResult.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    TotalCgstValue = groupedResult.Select(p => p.TotalCgstValue).DefaultIfEmpty().Sum(),
                    TotalSgstValue = groupedResult.Select(p => p.TotalSgstValue).DefaultIfEmpty().Sum(),
                    TotalUtgstValue = groupedResult.Select(p => p.TotalUtgstValue).DefaultIfEmpty().Sum(),
                    TotalIgstValue = groupedResult.Select(p => p.TotalIgstValue).DefaultIfEmpty().Sum(),
                    TotalCessValue = groupedResult.Select(p => p.TotalCessValue).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_16A(ClsSalesVm obj)
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

            var det = (from b in oConnectionContext.DbClsPurchase
                       where b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsCancelled == false && b.IsActive == true
        && oConnectionContext.DbClsBranch.Where(l => l.CompanyId == obj.CompanyId &&
        l.TaxSettingId == obj.TaxSettingId && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && DbFunctions.TruncateTime(b.PurchaseDate) >= obj.FromDate &&
        DbFunctions.TruncateTime(b.PurchaseDate) <= obj.ToDate && b.IsReverseCharge == 1
        && b.Status != "Draft" && b.GstTreatment == "Composition Taxable Supply"
                       select new
                       {
                           CustomerName = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.Name).FirstOrDefault(),
                           StateCode = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.StateCode).FirstOrDefault(),
                           SalesId = b.PurchaseId,
                           SalesDate = b.PurchaseDate,
                           InvoiceNo = b.ReferenceNo,
                           GrandTotal = b.GrandTotal,
                           PlaceOfSupply = oConnectionContext.DbClsState.Where(d => d.StateId == b.DestinationOfSupplyId).Select(d => d.State).FirstOrDefault(),
                           BusinessRegistrationNo = oConnectionContext.DbClsUser.Where(d => d.UserId == b.SupplierId).Select(d => d.BusinessRegistrationNo).FirstOrDefault(),
                           SalesDetails = (from c in oConnectionContext.DbClsPurchaseDetails
                                           where c.PurchaseId == b.PurchaseId && c.IsDeleted == false && c.IsActive == true
                                           && (c.TaxId != ExemptedId && c.TaxId != NonGstId)
                                           select new
                                           {
                                               //TaxPercent = oConnectionContext.DbClsTax.Where(d => d.TaxId == c.TaxId).Select(d => d.TaxPercent).FirstOrDefault(),
                                               AmountExcTax = c.AmountExcTax,
                                               //TaxTypes = (from x in oConnectionContext.DbClsTaxType
                                               //            select new
                                               //            {
                                               //                TaxTypeId = x.TaxTypeId,
                                               //                TaxType = x.TaxType,
                                               //                TaxAmount = (from y in oConnectionContext.DbClsTax
                                               //                             join z in oConnectionContext.DbClsPurchaseTaxJournal
                                               //                             on y.TaxId equals z.TaxId
                                               //                             where y.CompanyId == obj.CompanyId && x.TaxTypeId == y.TaxTypeId && z.PurchaseId == b.PurchaseId
                                               //                             && z.PurchaseDetailsId == c.PurchaseDetailsId
                                               //                             select z.TaxAmount
                                               //                            ).DefaultIfEmpty().Sum()
                                               //            }).ToList()
                                           }).ToList()

                       }).ToList();

            var groupedResult = det.Select(a => new ClsSalesVm
            {
                SalesDate = a.SalesDate,
                InvoiceNo = a.InvoiceNo,
                CustomerName = a.CustomerName,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                PlaceOfSupply = a.PlaceOfSupply,
                AmountExcTax = a.SalesDetails.Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                //TotalCgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                //TotalSgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "SGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                //TotalUtgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "UTGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                //TotalIgstValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "IGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                //TotalCessValue = a.SalesDetails.SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CESS").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                GrandTotal = a.GrandTotal,
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.OrderByDescending(a => a.SalesDate).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalInvoiceValue = det.Select(a => a.GrandTotal).DefaultIfEmpty().Sum(),
                    TotalTaxableValue = det.SelectMany(p => p.SalesDetails).Select(p => p.AmountExcTax).DefaultIfEmpty().Sum(),
                    //TotalCgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    //TotalSgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    //TotalUtgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    //TotalIgstValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    //TotalCessValue = det.SelectMany(p => p.SalesDetails).SelectMany(p => p.TaxTypes).Where(p => p.TaxType == "CGST").Select(p => p.TaxAmount).DefaultIfEmpty().Sum(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_17(ClsSalesVm obj)
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


            var det = oCommonController.HsnWiseOutwardSupplies(obj);

            var groupedResult = det
            .GroupBy(s => new { s.Unit, s.Code, s.TaxPercent })
            .Select(g => new
            {
                Code = g.Key.Code,
                Unit = g.Key.Unit,
                TaxPercent = g.Key.TaxPercent,
                ItemName = g.Select(b => b.ItemName).FirstOrDefault(),
                Quantity = g.Select(b => b.Quantity).Sum(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                GrandTotal = g.Select(b => b.GrandTotal).Sum(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_17Details(ClsSalesVm obj)
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

            var det = oCommonController.HsnWiseOutwardSupplies(obj).Where(a => a.Unit == obj.Unit && a.Code == obj.Code && a.TaxPercent == obj.TaxPercent);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_18(ClsSalesVm obj)
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


            var det = oCommonController.HsnWiseInwardSupplies(obj);

            var groupedResult = det
            .GroupBy(s => new { s.Unit, s.Code, s.TaxPercent })
            .Select(g => new
            {
                Code = g.Key.Code,
                Unit = g.Key.Unit,
                TaxPercent = g.Key.TaxPercent,
                ItemName = g.Select(b => b.ItemName).FirstOrDefault(),
                Quantity = g.Select(b => b.Quantity).Sum(),
                AmountExcTax = g.Select(b => b.AmountExcTax).Sum(),
                GrandTotal = g.Select(b => b.GrandTotal).Sum(),
                TaxTypes = g.SelectMany(s => s.TaxTypes)
                            .GroupBy(t => new { t.TaxTypeId, t.TaxType })
                            .Select(tg => new
                            {
                                TaxTypeId = tg.Key.TaxTypeId,
                                TaxType = tg.Key.TaxType,
                                TaxAmount = tg.Sum(t => t.TaxAmount)
                            })
                            .ToList()
            }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = groupedResult.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Gstr9_18Details(ClsSalesVm obj)
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

            var det = oCommonController.HsnWiseInwardSupplies(obj).Where(a => a.Unit == obj.Unit && a.Code == obj.Code && a.TaxPercent == obj.TaxPercent);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = det.Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        #endregion

    }
}
