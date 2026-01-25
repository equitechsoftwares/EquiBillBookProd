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

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class PaymentLinkController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        public async Task<IHttpActionResult> AllPaymentLinks(ClsPaymentLinkVm obj)
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

            var det = oConnectionContext.DbClsPaymentLink.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false
            && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate && DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate).Select(a => new
            {
                a.PaymentLinkId,
                a.Amount,
                a.ReferenceId,
                a.ReferenceNo,
                a.LinkExpirationDate,
                a.BranchId,
                a.CustomerId,
                CustomerName = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.Name).FirstOrDefault(),
                CustomerMobileNo = oConnectionContext.DbClsUser.Where(c => c.UserId == a.CustomerId).Select(c => c.MobileNo).FirstOrDefault(),
                a.Notes,
                a.Status,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
            }).ToList();

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.CustomerId == obj.CustomerId).ToList();
            }

            if (obj.Status != "" && obj.Status != null)
            {
                det = det.Where(a => a.Status == obj.Status).ToList();
            }

            if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
            {
                det = det.Where(a => a.ReferenceNo == obj.ReferenceNo).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentLinks = det.OrderByDescending(a => a.PaymentLinkId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentLink(ClsPaymentLink obj)
        {
            var det = oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.PaymentLinkId,
                a.Amount,
                a.ReferenceId,
                a.ReferenceNo,
                a.LinkExpirationDate,
                a.BranchId,
                a.CustomerId,
                a.Notes,
                a.Status,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                OnlinePaymentSettingsId = a.OnlinePaymentSettingsId,
                a.PlaceOfSupplyId,
                a.TaxId
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    PaymentLink = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertPaymentLink(ClsPaymentLinkVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                long PrefixUserMapId = 0;

                if (obj.CustomerId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCustomer" });
                    isError = true;
                }

                if (obj.Amount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                    isError = true;
                }

                if (obj.LinkExpirationDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divLinkExpirationDate" });
                    isError = true;
                }

                if (obj.ReferenceNo != "" && obj.ReferenceNo != null)
                {
                    if (oConnectionContext.DbClsPaymentLink.Where(a => a.ReferenceNo.ToLower() == obj.ReferenceNo.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Reference No exists", Id = "divReferenceNo" });
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

                //obj.BranchId = oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == obj.AddedBy && b.IsActive == true && b.IsDeleted == false).Select(b => b.BranchId).FirstOrDefault();

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    // Hybrid approach: Check Customer PrefixId first, then fall back to Branch PrefixId
                    long customerPrefixId = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => a.PrefixId).FirstOrDefault();
                    
                    if (customerPrefixId != 0)
                    {
                        // Use Customer's PrefixId if set
                        PrefixId = customerPrefixId;
                    }
                    else
                    {
                        // Fall back to Branch PrefixId (will be set below)
                        PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    }
                    
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == "payment link"
                                          && b.PrefixId == PrefixId
                                          select new
                                          {
                                              b.PrefixUserMapId,
                                              b.Prefix,
                                              b.NoOfDigits,
                                              b.Counter
                                          }).FirstOrDefault();
                    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                var userDet = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CustomerId && a.CompanyId == obj.CompanyId).Select(a => new
                {
                    a.IsBusinessRegistered,
                    a.GstTreatment,
                    a.BusinessRegistrationNameId,
                    a.BusinessRegistrationNo,
                    a.BusinessLegalName,
                    a.BusinessTradeName,
                    a.PanNo
                }).FirstOrDefault();

                ClsPaymentLink oPaymentLink = new ClsPaymentLink()
                {
                    ReferenceNo = obj.ReferenceNo,
                    CustomerId = obj.CustomerId,
                    Amount = obj.Amount,
                    LinkExpirationDate = obj.LinkExpirationDate.AddHours(5).AddMinutes(30),
                    Notes = obj.Notes,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    Status = "Generated",
                    ReferenceId = oCommonController.CreateToken(),
                    BranchId = obj.BranchId,
                    OnlinePaymentSettingsId = obj.OnlinePaymentSettingsId,
                    TaxId = obj.TaxId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    IsBusinessRegistered = userDet.IsBusinessRegistered,
                    GstTreatment = userDet.GstTreatment,
                    BusinessRegistrationNameId = userDet.BusinessRegistrationNameId,
                    BusinessRegistrationNo = userDet.BusinessRegistrationNo,
                    BusinessLegalName = userDet.BusinessLegalName,
                    BusinessTradeName = userDet.BusinessTradeName,
                    PanNo = userDet.PanNo,
                    PrefixId = PrefixId,
                    AccountId = obj.AccountId
                };
                oConnectionContext.DbClsPaymentLink.Add(oPaymentLink);
                oConnectionContext.SaveChanges();

                //increase counter
                string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                oConnectionContext.Database.ExecuteSqlCommand(q);
                //increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Link",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Link \"" + obj.ReferenceNo + "\" created",
                    Id = oPaymentLink.PaymentLinkId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Payment Link", obj.CompanyId, oPaymentLink.PaymentLinkId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Payment Link created successfully",
                    Data = new
                    {
                        PaymentLink = oPaymentLink
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdatePaymentLink(ClsPaymentLinkVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                //if (obj.Amount == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                //    isError = true;
                //}

                if (obj.LinkExpirationDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divLinkExpirationDate" });
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

                ClsPaymentLink oPaymentLink = new ClsPaymentLink()
                {
                    PaymentLinkId = obj.PaymentLinkId,
                    //CustomerId = obj.CustomerId,
                    //Amount = obj.Amount,
                    LinkExpirationDate = obj.LinkExpirationDate,
                    Notes = obj.Notes,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    Status = "Generated"
                };
                oConnectionContext.DbClsPaymentLink.Attach(oPaymentLink);
                oConnectionContext.Entry(oPaymentLink).Property(x => x.PaymentLinkId).IsModified = true;
                //oConnectionContext.Entry(oPaymentLink).Property(x => x.CustomerId).IsModified = true;
                //oConnectionContext.Entry(oPaymentLink).Property(x => x.Amount).IsModified = true;
                oConnectionContext.Entry(oPaymentLink).Property(x => x.LinkExpirationDate).IsModified = true;
                oConnectionContext.Entry(oPaymentLink).Property(x => x.Notes).IsModified = true;
                oConnectionContext.Entry(oPaymentLink).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oPaymentLink).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oPaymentLink).Property(x => x.Status).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Link",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Link \"" + oConnectionContext.DbClsPaymentLink.Where(a => a.PaymentLinkId == obj.PaymentLinkId).Select(a => a.ReferenceNo).FirstOrDefault() + "\" updated",
                    Id = oPaymentLink.PaymentLinkId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string[] arr = oNotificationTemplatesController.SendNotifications("Payment Link", obj.CompanyId, oPaymentLink.PaymentLinkId, 0, 0, 0, obj.AddedBy, CurrentDate, obj.Domain);

                data = new
                {
                    Status = 1,
                    Message = "Payment Link updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PaymentLinkDelete(ClsPaymentLinkVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var PaymentLink = oConnectionContext.DbClsPaymentLink.Where(a => a.CompanyId == obj.CompanyId &&
                a.PaymentLinkId == obj.PaymentLinkId && a.IsDeleted == false).Select(a => new
                {
                    a.Status,
                    a.ReferenceNo
                }).FirstOrDefault();

                if (PaymentLink.Status == "Paid")
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Cannot delete as it is already paid",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsPaymentLink oClsRole = new ClsPaymentLink()
                {
                    PaymentLinkId = obj.PaymentLinkId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsPaymentLink.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.PaymentLinkId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Payment Link",
                    CompanyId = obj.CompanyId,
                    Description = "Payment Link \"" + PaymentLink.ReferenceNo + "\" deleted",
                    Id = oClsRole.PaymentLinkId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Payment Link deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> SecurePayLink(ClsPaymentLinkVm obj)
        {
            var PaymentLink = oConnectionContext.DbClsPaymentLink.Where(a => a.ReferenceId == obj.ReferenceId && a.IsActive == true &&
            a.IsDeleted == false).Select(a => new ClsPaymentLinkVm
            {
                PaymentLinkId= a.PaymentLinkId,
                CustomerId=a.CustomerId,
                BranchId=a.BranchId,
                CompanyId=a.CompanyId,
                ReferenceNo=a.ReferenceNo,
                ReferenceId=a.ReferenceId,
                Amount=a.Amount,
                LinkExpirationDate=a.LinkExpirationDate,
                Status=a.Status,
                Notes=a.Notes,
                OnlinePaymentSettingsId=a.OnlinePaymentSettingsId
            }).FirstOrDefault();

            if (PaymentLink.LinkExpirationDate < oCommonController.CurrentDate(PaymentLink.CompanyId))
            {
                string query = "update \"tblPaymentLink\" set \"Status\"='Expired' where \"PaymentLinkId\"=" + PaymentLink.PaymentLinkId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                PaymentLink.Status = "Expired";
            }

            var User = oConnectionContext.DbClsUser.Where(c => c.UserId == PaymentLink.CustomerId).Select(c => new
            {
                c.Name,
                c.MobileNo,
                c.EmailId,
                Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == PaymentLink.CustomerId).Select(b => new
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
            }).FirstOrDefault();

            var Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == PaymentLink.BranchId).Select(b => new
            {
                b.Branch,
                Mobile = b.Mobile,
                b.Email,
                b.TaxNo,
                Tax = oConnectionContext.DbClsTax.Where(bb => bb.TaxId == b.TaxId).Select(bb => bb.Tax).FirstOrDefault(),
                b.Address,
                b.AltMobileNo,
                City = oConnectionContext.DbClsCity.Where(cc => cc.CityId == b.CityId).Select(cc => cc.City).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(cc => cc.StateId == b.StateId).Select(cc => cc.State).FirstOrDefault(),
                Country = oConnectionContext.DbClsCountry.Where(cc => cc.CountryId == b.CountryId).Select(cc => cc.Country).FirstOrDefault(),
                b.Zipcode
            }).FirstOrDefault();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == PaymentLink.CompanyId).Select(a => new
            {
                a.BusinessLogo,
                a.BusinessName,
                a.DateFormat,
                a.TimeFormat,
                a.CurrencySymbolPlacement,
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
            }).FirstOrDefault();

            var OnlinePaymentSetting = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == PaymentLink.CompanyId &&
            a.IsDefault == true && a.IsActive == true && a.IsDeleted == false).Select(a => new
            {
                a.OnlinePaymentSettingsId,
                a.RazorpayKey,
                a.RazorpayCurrencyId,
                a.OnlinePaymentService,
                a.PaypalClientId,
                a.PaypalCurrencyId,
                PaypalCurrencyCode = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.PaypalCurrencyId).Select(c => c.CurrencyCode).FirstOrDefault(),
                RazorpayCurrencyCode = oConnectionContext.DbClsCurrency.Where(c => c.CurrencyId == a.RazorpayCurrencyId).Select(c => c.CurrencyCode).FirstOrDefault(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = User,
                    Branch = Branch,
                    BusinessSetting = BusinessSetting,
                    PaymentLink = PaymentLink,
                    OnlinePaymentSetting = OnlinePaymentSetting
                }
            };
            return await Task.FromResult(Ok(data));
        }
    }
}
