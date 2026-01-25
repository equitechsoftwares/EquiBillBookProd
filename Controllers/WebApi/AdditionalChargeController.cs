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
using Vonage.Pricing;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class AdditionalChargeController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllAdditionalCharges(ClsAdditionalChargeVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }
            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false)
                .Select(a => new
                {
                    AdditionalChargeId = a.AdditionalChargeId,
                    Name = a.Name,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    CompanyId = a.CompanyId,
                    ItemCodeId = a.ItemCodeId,
                    TaxPreferenceId = a.TaxPreferenceId,
                    TaxExemptionId = a.TaxExemptionId,
                    IntraStateTaxId = a.IntraStateTaxId,
                    InterStateTaxId = a.InterStateTaxId,
                    PurchaseAccountId = a.PurchaseAccountId,
                    SalesAccountId = a.SalesAccountId,
                    Description = a.Description,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                }).ToList();

            if (!string.IsNullOrEmpty(obj.Search))
            {
                det = det.Where(a => a.Name.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AdditionalCharges = det.OrderByDescending(a => a.AdditionalChargeId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AdditionalCharge(ClsAdditionalCharge obj)
        {
            var det = oConnectionContext.DbClsAdditionalCharge.Where(a => a.AdditionalChargeId == obj.AdditionalChargeId && a.CompanyId == obj.CompanyId)
                .Select(a => new
                {
                    TaxPreference = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxPreferenceId && b.CompanyId == obj.CompanyId).Select(b => b.Tax).FirstOrDefault(),
                    AdditionalChargeId = a.AdditionalChargeId,
                    Name = a.Name,
                    IsActive = a.IsActive,
                    IsDeleted = a.IsDeleted,
                    AddedBy = a.AddedBy,
                    AddedOn = a.AddedOn,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedOn = a.ModifiedOn,
                    CompanyId = a.CompanyId,
                    ItemCodeId = a.ItemCodeId,
                    TaxPreferenceId = a.TaxPreferenceId,
                    TaxExemptionId = a.TaxExemptionId,
                    IntraStateTaxId = a.IntraStateTaxId,
                    InterStateTaxId = a.InterStateTaxId,
                    PurchaseAccountId = a.PurchaseAccountId,
                    SalesAccountId = a.SalesAccountId,
                    Description = a.Description,
                    AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new { AdditionalCharge = det }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertAdditionalCharge(ClsAdditionalChargeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (string.IsNullOrEmpty(obj.Name))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                    isError = true;
                }
                if (oConnectionContext.DbClsAdditionalCharge.Any(a => a.Name.ToLower() == obj.Name.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false))
                {
                    errors.Add(new ClsError { Message = "Duplicate Additional Charge Name exists", Id = "divName" });
                    isError = true;
                }
                if (obj.TaxPreference == "Taxable")
                {
                    if (obj.IntraStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divIntraStateTax" });
                        isError = true;
                    }

                    if (obj.InterStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divInterStateTax" });
                        isError = true;
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

                ClsMenuVm oClsMenuVm = new ClsMenuVm
                {
                    AddedBy = obj.AddedBy,
                    CompanyId = obj.CompanyId,
                };
                var PlanAddons = oCommonController.PlanAddons(oClsMenuVm);

                if (PlanAddons != null && PlanAddons.Where(a => a.Type == "Accounts").Count() > 0)
                {
                    if (PlanAddons.Where(a => a.Type == "Purchase").Count() > 0)
                    {
                        if (obj.PurchaseAccountId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseAccount" });
                            isError = true;
                        }
                    }
                    if (obj.SalesAccountId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesAccount" });
                        isError = true;
                    }
                }

                if (isError)
                {
                    data = new { Status = 2, Message = "", Errors = errors, Data = new { } };
                    return await Task.FromResult(Ok(data));
                }

                ClsAdditionalCharge oAdditionalCharge = new ClsAdditionalCharge()
                {
                    Name = obj.Name,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    ItemCodeId = obj.ItemCodeId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    TaxExemptionId = obj.TaxExemptionId,
                    IntraStateTaxId = obj.IntraStateTaxId,
                    InterStateTaxId = obj.InterStateTaxId,
                    PurchaseAccountId = obj.PurchaseAccountId,
                    SalesAccountId = obj.SalesAccountId,
                    Description = obj.Description
                };
                oConnectionContext.DbClsAdditionalCharge.Add(oAdditionalCharge);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Additional Charge",
                    CompanyId = obj.CompanyId,
                    Description = "Additional Charge \"" + obj.Name + "\" created",
                    Id = oAdditionalCharge.AdditionalChargeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Additional Charge created successfully",
                    Data = new { AdditionalCharge = oAdditionalCharge }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateAdditionalCharge(ClsAdditionalChargeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (string.IsNullOrEmpty(obj.Name))
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                    isError = true;
                }
                if (oConnectionContext.DbClsAdditionalCharge.Any(a => a.Name.ToLower() == obj.Name.ToLower() && a.CompanyId == obj.CompanyId && a.AdditionalChargeId != obj.AdditionalChargeId && a.IsDeleted == false))
                {
                    errors.Add(new ClsError { Message = "Duplicate Additional Charge Name exists", Id = "divName" });
                    isError = true;
                }
                if (obj.TaxPreference == "Taxable")
                {
                    if (obj.IntraStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divIntraStateTax" });
                        isError = true;
                    }

                    if (obj.InterStateTaxId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divInterStateTax" });
                        isError = true;
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

                ClsMenuVm oClsMenuVm = new ClsMenuVm
                {
                    AddedBy = obj.AddedBy,
                    CompanyId = obj.CompanyId,
                };
                var PlanAddons = oCommonController.PlanAddons(oClsMenuVm);

                if (PlanAddons != null && PlanAddons.Where(a => a.Type == "Accounts").Count() > 0)
                {
                    if (PlanAddons.Where(a => a.Type == "Purchase").Count() > 0)
                    {
                        if (obj.PurchaseAccountId == 0)
                        {
                            errors.Add(new ClsError { Message = "This field is required", Id = "divPurchaseAccount" });
                            isError = true;
                        }
                    }
                    if (obj.SalesAccountId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divSalesAccount" });
                        isError = true;
                    }
                }

                if (isError)
                {
                    data = new { Status = 2, Message = "", Errors = errors, Data = new { } };
                    return await Task.FromResult(Ok(data));
                }

                ClsAdditionalCharge oAdditionalCharge = new ClsAdditionalCharge()
                {
                    AdditionalChargeId = obj.AdditionalChargeId,
                    Name = obj.Name,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = CurrentDate,
                    ItemCodeId = obj.ItemCodeId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    TaxExemptionId = obj.TaxExemptionId,
                    IntraStateTaxId = obj.IntraStateTaxId,
                    InterStateTaxId = obj.InterStateTaxId,
                    PurchaseAccountId = obj.PurchaseAccountId,
                    SalesAccountId = obj.SalesAccountId,
                    Description = obj.Description,
                };
                oConnectionContext.DbClsAdditionalCharge.Attach(oAdditionalCharge);
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.Name).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ItemCodeId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.TaxPreferenceId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.IntraStateTaxId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.InterStateTaxId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.PurchaseAccountId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.SalesAccountId).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.Description).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.ModifiedBy,
                    Browser = obj.Browser,
                    Category = "Additional Charge",
                    CompanyId = obj.CompanyId,
                    Description = "Additional Charge \"" + obj.Name + "\" updated",
                    Id = oAdditionalCharge.AdditionalChargeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Additional Charge updated successfully",
                    Data = new { AdditionalCharge = oAdditionalCharge }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AdditionalChargeActiveInactive(ClsAdditionalChargeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAdditionalCharge oAdditionalCharge = new ClsAdditionalCharge()
                {
                    AdditionalChargeId = obj.AdditionalChargeId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId
                };
                oConnectionContext.DbClsAdditionalCharge.Attach(oAdditionalCharge);
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.AdditionalChargeId == obj.AdditionalChargeId).Select(a => new { a.Name }).FirstOrDefault();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.ModifiedBy,
                    Browser = obj.Browser,
                    Category = "Additional Charge",
                    CompanyId = obj.CompanyId,
                    Description = "Additional Charge \"" + (AdditionalCharge != null ? AdditionalCharge.Name : "") + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oAdditionalCharge.AdditionalChargeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Status Change"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new { Status = 1, Message = "Status updated successfully", Data = new { AdditionalCharge = oAdditionalCharge } };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AdditionalChargeDelete(ClsAdditionalChargeVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsAdditionalCharge oAdditionalCharge = new ClsAdditionalCharge()
                {
                    AdditionalChargeId = obj.AdditionalChargeId,
                    IsDeleted = true,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = CurrentDate,
                    CompanyId = obj.CompanyId
                };
                oConnectionContext.DbClsAdditionalCharge.Attach(oAdditionalCharge);
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oAdditionalCharge).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var AdditionalCharge = oConnectionContext.DbClsAdditionalCharge.Where(a => a.AdditionalChargeId == obj.AdditionalChargeId).Select(a => new { a.Name }).FirstOrDefault();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.ModifiedBy,
                    Browser = obj.Browser,
                    Category = "Additional Charge",
                    CompanyId = obj.CompanyId,
                    Description = "Additional Charge \"" + (AdditionalCharge != null ? AdditionalCharge.Name : "") + "\" deleted",
                    Id = oAdditionalCharge.AdditionalChargeId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new { Status = 1, Message = "Additional Charge deleted successfully", Data = new { AdditionalCharge = oAdditionalCharge } };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveAdditionalCharges(ClsAdditionalChargeVm obj)
        {
            string GstTreatment = "";
            if (obj.CustomerId != 0)
            {
                GstTreatment = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId).Select(a => a.GstTreatment).FirstOrDefault();
            }
            else
            {
                GstTreatment = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.SupplierId).Select(a => a.GstTreatment).FirstOrDefault();
            }

            long BranchStateId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.StateId).FirstOrDefault();

            var det = oConnectionContext.DbClsAdditionalCharge.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true)
            .Select(a => new ClsAdditionalChargeVm
            {
                AdditionalChargeId = a.AdditionalChargeId,
                Name = a.Name,
                ItemCodeId = a.ItemCodeId,
                TaxPreferenceId = a.TaxPreferenceId,
                TaxExemptionId = a.TaxExemptionId,
                IntraStateTaxId = a.IntraStateTaxId,
                InterStateTaxId = a.InterStateTaxId,
                PurchaseAccountId = a.PurchaseAccountId,
                SalesAccountId = a.SalesAccountId,
                GstTreatment = GstTreatment,
                ShortCutKey = a.ShortCutKey
            }).ToList();

            if (obj.IsBillOfSupply == true)
            {
                long TaxId = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.Tax == "Non-Taxable").Select(a => a.TaxId).FirstOrDefault();

                foreach (var item in det)
                {
                    item.IntraStateTaxId = TaxId;
                    item.InterStateTaxId = TaxId;
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    AdditionalCharges = det,
                    BusinessSetting = new
                    {
                        GstTreatment = GstTreatment,
                        StateId = BranchStateId
                    }
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
