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
    public class OnlinePaymentSettingsController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        dynamic data = null;

        public async Task<IHttpActionResult> AllOnlinePaymentSettings(ClsOnlinePaymentSettingsVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                OnlinePaymentService = a.OnlinePaymentService,
                RazorpayKey = a.RazorpayKey,
                RazorpayCurrencyId = a.RazorpayCurrencyId,
                PaypalClientId = a.PaypalClientId,
                PaypalCurrencyId = a.PaypalCurrencyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.AddedOn,
                a.ModifiedOn,
                a.IsDefault,
                a.IsActive,
                a.SaveAs
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.SaveAs.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OnlinePaymentSettings = det.OrderByDescending(a => a.OnlinePaymentSettingsId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> OnlinePaymentSetting(ClsOnlinePaymentSettingsVm obj)
        {
            var det = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId && a.OnlinePaymentSettingsId == obj.OnlinePaymentSettingsId).Select(a => new
            {
                OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                OnlinePaymentService = a.OnlinePaymentService,
                RazorpayKey = a.RazorpayKey,
                RazorpayCurrencyId = a.RazorpayCurrencyId,
                PaypalClientId = a.PaypalClientId,
                PaypalCurrencyId = a.PaypalCurrencyId,
                a.IsDefault,
                a.SaveAs
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OnlinePaymentSetting = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertOnlinePaymentSetting(ClsOnlinePaymentSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);


                if (oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.SaveAs.ToLower() == obj.SaveAs.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate name", Id = "divPaymentServiceSaveAs" });
                    isError = true;
                }

                if (obj.OnlinePaymentService == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divOnlinePaymentService" });
                    isError = true;
                }

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentServiceSaveAs" });
                    isError = true;
                }

                //if (obj.OnlinePaymentService != 0)
                //{
                //    if (oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.OnlinePaymentService == obj.OnlinePaymentService
                //     && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = (obj.OnlinePaymentService == 1 ? "Paypal" : "Razorpay") + " is already added", Id = "divOnlinePaymentService" });
                //        isError = true;

                //        data = new
                //        {
                //            Status = 2,
                //            Message = "",
                //            Errors = errors,
                //            Data = new
                //            {
                //            }
                //        };
                //        return await Task.FromResult(Ok(data));
                //    }
                //}

                if (obj.OnlinePaymentService == 1)
                {
                    if (obj.PaypalClientId == "" || obj.PaypalClientId == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPaypalClientId" });
                        isError = true;
                    }
                    if (obj.PaypalCurrencyId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPaypalCurrency" });
                        isError = true;
                    }
                }

                if (obj.OnlinePaymentService == 2)
                {
                    if (obj.RazorpayKey == "" || obj.RazorpayKey == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divRazorpayKey" });
                        isError = true;
                    }
                    if (obj.RazorpayCurrencyId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divRazorpayCurrency" });
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

                ClsOnlinePaymentSettings oClsOnlinePaymentSettings = new ClsOnlinePaymentSettings()
                {
                    IsActive = true,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    OnlinePaymentService = obj.OnlinePaymentService,
                    ModifiedBy = obj.ModifiedBy,
                    ModifiedOn = obj.ModifiedOn,
                    PaypalClientId = obj.PaypalClientId,
                    RazorpayKey = obj.RazorpayKey,
                    RazorpayCurrencyId = obj.RazorpayCurrencyId,
                    PaypalCurrencyId = obj.PaypalCurrencyId,
                    IsDefault = obj.IsDefault,
                    SaveAs = obj.SaveAs,
                    IsDeleted = false
                };

                oConnectionContext.DbClsOnlinePaymentSettings.Add(oClsOnlinePaymentSettings);
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblOnlinePaymentSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"OnlinePaymentSettingsId\"!=" + oClsOnlinePaymentSettings.OnlinePaymentSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Online Payments\" created",
                    Id = oClsOnlinePaymentSettings.OnlinePaymentSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Online Payment created successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateOnlinePaymentSetting(ClsOnlinePaymentSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.SaveAs == null || obj.SaveAs == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentServiceSaveAs" });
                    isError = true;
                }

                if (obj.OnlinePaymentService == 1)
                {
                    if (obj.PaypalClientId == "" || obj.PaypalClientId == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPaypalClientId" });
                        isError = true;
                    }
                    if (obj.PaypalCurrencyId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPaypalCurrency" });
                        isError = true;
                    }
                }

                if (obj.OnlinePaymentService == 2)
                {
                    if (obj.RazorpayKey == "" || obj.RazorpayKey == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divRazorpayKey" });
                        isError = true;
                    }
                    if (obj.RazorpayCurrencyId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divRazorpayCurrency" });
                        isError = true;
                    }
                }

                if (oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.SaveAs.ToLower() == obj.SaveAs.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId&& a.OnlinePaymentSettingsId != obj.OnlinePaymentSettingsId).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate name", Id = "divPaymentServiceSaveAs" });
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

                ClsOnlinePaymentSettings oClsOnlinePaymentSettings = new ClsOnlinePaymentSettings()
                {
                    OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    //OnlinePaymentService = obj.OnlinePaymentService,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    PaypalClientId = obj.PaypalClientId,
                    RazorpayKey = obj.RazorpayKey,
                    RazorpayCurrencyId = obj.RazorpayCurrencyId,
                    PaypalCurrencyId = obj.PaypalCurrencyId,
                    IsDefault = obj.IsDefault,
                    SaveAs = obj.SaveAs,
                };

                oConnectionContext.DbClsOnlinePaymentSettings.Attach(oClsOnlinePaymentSettings);
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.OnlinePaymentSettingsId).IsModified = true;
                //oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.OnlinePaymentService).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.PaypalClientId).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.RazorpayKey).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.RazorpayCurrencyId).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.PaypalCurrencyId).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.IsDefault).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.SaveAs).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblOnlinePaymentSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId + " and \"OnlinePaymentSettingsId\"!=" + obj.OnlinePaymentSettingsId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Online Payments\" update",
                    Id = oClsOnlinePaymentSettings.OnlinePaymentSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Business Settings"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Online Payment updated successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        public async Task<IHttpActionResult> OnlinePaymentSettingActiveInactive(ClsOnlinePaymentSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsOnlinePaymentSettings oClsOnlinePaymentSettings = new ClsOnlinePaymentSettings()
                {
                    OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsOnlinePaymentSettings.Attach(oClsOnlinePaymentSettings);
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.OnlinePaymentSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId
               && a.OnlinePaymentSettingsId == obj.OnlinePaymentSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long OnlinePaymentSettingsId = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.OnlinePaymentSettingsId != obj.OnlinePaymentSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.OnlinePaymentSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblOnlinePaymentSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (OnlinePaymentSettingsId != 0)
                    {
                        string query1 = "update \"tblOnlinePaymentSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"OnlinePaymentSettingsId\"=" + OnlinePaymentSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"" + oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.OnlinePaymentSettingsId == obj.OnlinePaymentSettingsId).Select(a => a.OnlinePaymentService).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsOnlinePaymentSettings.OnlinePaymentSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Online Payment " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> OnlinePaymentSettingDelete(ClsOnlinePaymentSettingsVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsOnlinePaymentSettings oClsOnlinePaymentSettings = new ClsOnlinePaymentSettings()
                {
                    OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsOnlinePaymentSettings.Attach(oClsOnlinePaymentSettings);
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.OnlinePaymentSettingsId).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsOnlinePaymentSettings).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                obj.IsDefault = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId
               && a.OnlinePaymentSettingsId == obj.OnlinePaymentSettingsId).Select(a => a.IsDefault).FirstOrDefault();

                long OnlinePaymentSettingsId = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId
                && a.OnlinePaymentSettingsId != obj.OnlinePaymentSettingsId && a.IsDeleted == false && a.IsActive == true).Select(a => a.OnlinePaymentSettingsId).FirstOrDefault();

                if (obj.IsDefault == true)
                {
                    string query = "update \"tblOnlinePaymentSettings\" set \"IsDefault\"=False where \"CompanyId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    if (OnlinePaymentSettingsId != 0)
                    {
                        string query1 = "update \"tblOnlinePaymentSettings\" set \"IsDefault\"=True where \"CompanyId\"=" + obj.CompanyId + " and \"OnlinePaymentSettingsId\"=" + OnlinePaymentSettingsId;
                        oConnectionContext.Database.ExecuteSqlCommand(query1);
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"Online Payments\" deleted",
                    Id = oClsOnlinePaymentSettings.OnlinePaymentSettingsId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Online Payment deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveOnlinePaymentSettings(ClsOnlinePaymentSettingsVm obj)
        {
            var det = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.IsDeleted == false && a.IsActive == true && a.CompanyId == obj.CompanyId).Select(a => new
            {
                OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                OnlinePaymentService = a.OnlinePaymentService,
                RazorpayKey = a.RazorpayKey,
                RazorpayCurrencyId = a.RazorpayCurrencyId,
                PaypalClientId = a.PaypalClientId,
                PaypalCurrencyId = a.PaypalCurrencyId,
                a.IsDefault,
                a.SaveAs,
                PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(b=>b.CurrencyId==a.PaypalCurrencyId).Select(b=>b.CurrencyCode).FirstOrDefault(),
                RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.RazorpayCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault()
            }).OrderBy(a => a.SaveAs).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OnlinePaymentSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ActiveOnlinePaymentSettingsAdmin(ClsOnlinePaymentSettingsVm obj)
        {
            //obj.CompanyId = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            obj.Under = oConnectionContext.DbClsUser.Where(a=>a.UserId == oConnectionContext.DbClsDomain.Where(aa => aa.Domain == obj.Domain && aa.IsDeleted == false && aa.IsActive == true).Select(aa => aa.CompanyId).FirstOrDefault()).Select(a=>a.Under).FirstOrDefault();

            var det = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.IsDeleted == false && a.IsActive == true && a.CompanyId == obj.Under).Select(a => new
            {
                OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                OnlinePaymentService = a.OnlinePaymentService,
                RazorpayKey = a.RazorpayKey,
                RazorpayCurrencyId = a.RazorpayCurrencyId,
                PaypalClientId = a.PaypalClientId,
                PaypalCurrencyId = a.PaypalCurrencyId,
                a.IsDefault,
                a.SaveAs,
                PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.PaypalCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.RazorpayCurrencyId).Select(b => b.CurrencyCode).FirstOrDefault()
            }).OrderBy(a => a.SaveAs).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    OnlinePaymentSettings = det,
                }
            };

            return await Task.FromResult(Ok(data));
        }

    }
}
