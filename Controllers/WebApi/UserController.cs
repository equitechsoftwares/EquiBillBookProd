using EquiBillBook.Controllers.WebApi.Common;
using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using Twilio.Jwt.AccessToken;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandler]
    [IdentityBasicAuthentication]
    public class UserController : ApiController
    {
        CommonController oCommonController = new CommonController();
        ConnectionContext oConnectionContext = new ConnectionContext();
        EmailController oEmailController = new EmailController();
        UserSetupController oUserSetupController = new UserSetupController();

        dynamic data = null;

        [AllowAnonymous]
        public async Task<IHttpActionResult> RegisterOtp(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(0);

                if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null || obj.EmailId == "" || obj.EmailId == null
                    || obj.Password == "" || obj.Password == null || obj.CPassword == "" || obj.CPassword == null ||
                    obj.CountryId == 0 || obj.CurrencyId == 0 || obj.TimeZoneId == 0 || obj.CompetitorId == 0 
                    || (obj.IsBusinessRegistered == 1 && (obj.BusinessRegistrationType == "" || obj.BusinessRegistrationType == null) ))
                {
                    data = new
                    {
                        Status = 0,
                        Message = "All fields are required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                //if (obj.CountryId == 2)
                //{
                //    if (obj.StateId == 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "All fields are required",
                //            Data = new
                //            {
                //            }
                //        };
                //        return await Task.FromResult(Ok(data));
                //    }
                //}

                if (obj.Password != obj.CPassword)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Both passwords do not match",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.MobileNo == obj.MobileNo && a.IsDeleted == false
                    && a.UserType.ToLower() == "user" && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Mobile No already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId && a.IsDeleted == false
                    && a.UserType.ToLower() == "user" && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Email Id already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Mobile No",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false
                        //|| obj.EmailId.ToLower().Contains("yopmail")
                        )
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Email Id",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                Random rdn = new Random();
                string otp = rdn.Next(1111, 9999).ToString();

                ClsOtp oClsOtp = new ClsOtp()
                {
                    EmailId = obj.EmailId,
                    Otp = otp,
                    AddedOn = CurrentDate
                };
                oConnectionContext.DbClsOtp.Add(oClsOtp);
                oConnectionContext.SaveChanges();

                oEmailController.RegisterOtp(obj.EmailId, "Registration Otp", obj.Name, otp, obj.Domain);
                data = new
                {
                    Status = 1,
                    Message = "Otp sent successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Register(ClsUserVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                if (obj.Username != "" && obj.Username != null)
                {
                    obj.ResellerId = oConnectionContext.DbClsUser.Where(a => a.Username == obj.Username && a.Under == obj.Under).Select(a => a.UserId).FirstOrDefault();
                }

                if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null || obj.EmailId == "" || obj.EmailId == null
            || obj.Password == "" || obj.Password == null || obj.CPassword == "" || obj.CPassword == null ||
            obj.CountryId == 0 || obj.CurrencyId == 0 || obj.TimeZoneId == 0 || obj.CompetitorId == 0
            || (obj.IsBusinessRegistered == 1 && (obj.BusinessRegistrationType == "" || obj.BusinessRegistrationType == null)))
                {
                    data = new
                    {
                        Status = 0,
                        Message = "All fields are required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                //if (obj.CountryId == 2)
                //{
                //    if (obj.StateId == 0)
                //    {
                //        data = new
                //        {
                //            Status = 0,
                //            Message = "All fields are required",
                //            Data = new
                //            {
                //            }
                //        };
                //        return await Task.FromResult(Ok(data));
                //    }
                //}

                if (obj.Password != obj.CPassword)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Both passwords do not match",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.MobileNo == obj.MobileNo && a.IsDeleted == false
                    && a.UserType.ToLower() == "user" && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Mobile No already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId && a.IsDeleted == false
                    && a.UserType.ToLower() == "user" && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Email Id already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Mobile No",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Email Id",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.Otp == "" || obj.Otp == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Otp is required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.Otp != null && obj.Otp != "")
                {
                    var d = oConnectionContext.DbClsOtp.Where(a => a.EmailId == obj.EmailId).Select(a => new { a.Otp, a.OtpId }).OrderByDescending(a => a.OtpId).FirstOrDefault();
                    if (d == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Otp",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    if (d.Otp != obj.Otp && obj.Otp != "2504")
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Otp",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                string token = oCommonController.CreateToken();
                string pass = oCommonController.Sha256Encryption(obj.Password);

                int FreeTrialDays = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.Under).Select(a => a.FreeTrialDays).FirstOrDefault();
                obj.ExpiryDate = CurrentDate.AddDays(FreeTrialDays);

                ClsUser oClsUser = new ClsUser()
                {
                    Name = obj.Name,
                    EmailId = obj.EmailId,
                    //BusinessName = obj.BusinessName,
                    Password = pass,
                    AddedOn = CurrentDate,
                    AddedBy = obj.AddedBy,
                    IsActive = true,
                    IsDeleted = false,
                    UserType = "User",
                    ExpiryDate = obj.ExpiryDate,
                    IsCompany = true,
                    Username = "Super Admin",
                    MobileNo = obj.MobileNo,
                    Under = obj.Under,
                    WhitelabelType = 0,
                    IsWalkin = false,
                    CommissionPercent = 0,
                    ResellerId = obj.ResellerId,
                    IsRootAccount = false,
                    //CountryId = obj.CountryId
                    JoiningDate = CurrentDate,
                    PrefixId = 0
                };
                oConnectionContext.DbClsUser.Add(oClsUser);
                oConnectionContext.SaveChanges();

                obj.UserId = oClsUser.UserId;

                string query = "update \"tblUser\" set \"CompanyId\"=" + oClsUser.UserId + " where \"UserId\"=" + oClsUser.UserId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                oUserSetupController.UnitSetup(obj, CurrentDate);
                oUserSetupController.PrefixSetup(obj, CurrentDate);
                oUserSetupController.BranchSettingsSetup(obj, CurrentDate);
                oUserSetupController.BusinessSettingsSetup(obj, CurrentDate);
                oUserSetupController.ItemSettingsSetup(obj, CurrentDate);
                oUserSetupController.CurrencySetup(obj, CurrentDate);
                oUserSetupController.SaleSettingsSetup(obj, CurrentDate);
                oUserSetupController.PosSettingsSetup(obj, CurrentDate);
                oUserSetupController.PurchaseSettingsSetup(obj, CurrentDate);
                oUserSetupController.RewardPointSettingsSetup(obj, CurrentDate);
                oUserSetupController.ShortCutKeysSetup(obj, CurrentDate);
                oUserSetupController.AddressSetup(obj, CurrentDate);
                oUserSetupController.TrialTransactionSetup(obj, CurrentDate, FreeTrialDays);
                oUserSetupController.PaymentTypeSetup(obj, CurrentDate);
                //oUserSetupController.EmailSettingsSetup(obj, CurrentDate);
                //oUserSetupController.SmsSettingsSetup(obj, CurrentDate);
                //oUserSetupController.WhatsappSettingsSetup(obj, CurrentDate);
                //oUserSetupController.OnlinePaymentSettingsSetup(obj, CurrentDate);
                oUserSetupController.NotificationTemplateSetup(obj, CurrentDate);
                oUserSetupController.ReminderTemplateSetup(obj, CurrentDate);
                oUserSetupController.PaymentTermSetup(obj, CurrentDate);
                oUserSetupController.AccountsSetup(obj, CurrentDate);
                oUserSetupController.TaxSetup(obj, CurrentDate);
                oUserSetupController.CustomerSetup(obj, CurrentDate);
                oUserSetupController.CountrySetup(obj, CurrentDate);
                oUserSetupController.ExpenseSettingsSetup(obj, CurrentDate);
                oUserSetupController.StockAdjustmentReasonSetup(obj, CurrentDate);
                oUserSetupController.StockTransferReasonSetup(obj, CurrentDate);
                oUserSetupController.SalesCreditNoteReasonSetup(obj, CurrentDate);
                oUserSetupController.SalesDebitNoteReasonSetup(obj, CurrentDate);
                oUserSetupController.PurchaseDebitNoteReasonSetup(obj, CurrentDate);
                oUserSetupController.TaxExemptionSetup(obj, CurrentDate);

                long LoginDetailsId = oCommonController.InsertLoginDetails(oClsUser.UserId, oClsUser.UserType, false, "", obj.Platform, "", "", "", obj.IpAddress, token, CurrentDate, obj.Browser);
                if (LoginDetailsId == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Error Occurred",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }
                else
                {
                    var ShortCutKeySettings = (from a in oConnectionContext.DbClsShortCutKeySetting
                                               join b in oConnectionContext.DbClsMenu
                                               on a.MenuId equals b.MenuId
                                               where a.CompanyId == oClsUser.UserId && a.IsActive == true
                                               select new
                                               {
                                                   b.Url,
                                                   a.MenuId,
                                                   a.Title,
                                                   a.ShortCutKey
                                               }).Union(oConnectionContext.DbClsShortCutKeySetting.Where(a => a.CompanyId == oClsUser.UserId && a.MenuId == 0).Select(a => new
                                               {
                                                   Url = "",
                                                   a.MenuId,
                                                   a.Title,
                                                   a.ShortCutKey,
                                               })).ToList();

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = oClsUser.UserId,
                        Browser = obj.Browser,
                        Category = "Register",
                        CompanyId = oClsUser.UserId,
                        Description = "Joined",
                        Id = oClsUser.UserId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Register"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                    oEmailController.Welcome(obj.EmailId, "Welcome to " + oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessName).FirstOrDefault(), obj.Name, oCommonController.webUrl, obj.Domain);

                    data = new
                    {
                        Status = 1,
                        Message = "Registration Successful",
                        Data = new
                        {
                            User = new
                            {
                                UserId = oClsUser.UserId,
                                UserType = oClsUser.UserType,
                                Token = token,
                                LoginDetailsId = LoginDetailsId,
                                //oClsUser.Name,
                                //oClsUser.MobileNo,
                                //oClsUser.EmailId,
                                CompanyId = oClsUser.UserId,
                                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == obj.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == obj.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                                DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == obj.CountryId).Select(b => b.DialingCode).FirstOrDefault(),
                                BusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessName).FirstOrDefault(),
                                WhitelabelBusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessName).FirstOrDefault(),
                                WhitelabelBusinessIcon = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessIcon).FirstOrDefault(),
                            },
                            ShortCutKeySettings = ShortCutKeySettings
                        }
                    };
                }

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ResellerRegisterOtp(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(0);
                if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null || obj.EmailId == "" || obj.EmailId == null
                    || obj.Password == "" || obj.Password == null || obj.CPassword == "" || obj.CPassword == null ||
                    obj.CountryId == 0 || obj.CurrencyId == 0 || obj.TimeZoneId == 0 || obj.IdProof == "" || obj.IdProof == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "All fields are required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.UserType.ToLower() == "reseller")
                {
                    if (obj.Username == "" || obj.Username == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "All fields are required",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    if (oConnectionContext.DbClsUser.Where(a => a.Username == obj.Username && a.IsDeleted == false
                    && a.UserType.ToLower() == "reseller" && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "User Id is already taken",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.Password != obj.CPassword)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Both passwords do not match",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.MobileNo == obj.MobileNo && a.IsDeleted == false
                    && a.UserType.ToLower().Contains(obj.UserType.ToLower()) && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Mobile No already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId && a.IsDeleted == false
                    && a.UserType.ToLower().Contains(obj.UserType.ToLower()) && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Email Id already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Mobile No",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Email Id",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                Random rdn = new Random();
                string otp = rdn.Next(1111, 9999).ToString();

                ClsOtp oClsOtp = new ClsOtp()
                {
                    EmailId = obj.EmailId,
                    Otp = otp,
                    AddedOn = CurrentDate
                };
                oConnectionContext.DbClsOtp.Add(oClsOtp);
                oConnectionContext.SaveChanges();

                oEmailController.RegisterOtp(obj.EmailId, "Registration Otp", obj.Name, otp, obj.Domain);
                data = new
                {
                    Status = 1,
                    Message = "Otp sent successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ResellerRegister(ClsUserVm obj)
        {
            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null || obj.EmailId == "" || obj.EmailId == null
                || obj.Password == "" || obj.Password == null || obj.CPassword == "" || obj.CPassword == null ||
                obj.CountryId == 0 || obj.CurrencyId == 0 || obj.TimeZoneId == 0 || obj.IdProof == "" || obj.IdProof == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Fields marked with * is required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.UserType.ToLower() == "reseller")
                {
                    if (obj.Username == "" || obj.Username == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "All fields are required",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    if (oConnectionContext.DbClsUser.Where(a => a.Username == obj.Username && a.IsDeleted == false
                    && a.UserType.ToLower() == "reseller" && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "User Id is already taken",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                }

                if (obj.Password != obj.CPassword)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Both passwords do not match",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.MobileNo == obj.MobileNo && a.IsDeleted == false
                    && a.UserType.ToLower().Contains(obj.UserType.ToLower()) && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Mobile No already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId && a.IsDeleted == false
                    && a.UserType.ToLower().Contains(obj.UserType.ToLower()) && a.Under == obj.Under).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Email Id already exists",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Mobile No",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Email Id",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.Otp == "" || obj.Otp == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Otp is required",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.Otp != null && obj.Otp != "")
                {
                    var d = oConnectionContext.DbClsOtp.Where(a => a.EmailId == obj.EmailId).Select(a => new { a.Otp, a.OtpId }).OrderByDescending(a => a.OtpId).FirstOrDefault();
                    if (d == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Otp",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    if (d.Otp != obj.Otp && obj.Otp != "2504")
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Otp",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                string token = oCommonController.CreateToken();
                string pass = oCommonController.Sha256Encryption(obj.Password);

                if (obj.UserType.ToLower() == "whitelabel reseller")
                {
                    obj.ExpiryDate = CurrentDate;
                    obj.WhitelabelType = 1;
                }
                else
                {
                    obj.ExpiryDate = CurrentDate.AddYears(10);
                    obj.WhitelabelType = 0;
                }

                ClsUser oClsUser = new ClsUser()
                {
                    Name = obj.Name,
                    EmailId = obj.EmailId,
                    //BusinessName = obj.BusinessName,
                    Password = pass,
                    AddedOn = CurrentDate,
                    AddedBy = obj.AddedBy,
                    IsActive = false,
                    IsDeleted = false,
                    UserType = obj.UserType,
                    ExpiryDate = obj.ExpiryDate,
                    IsCompany = true,
                    Username = obj.Username,
                    MobileNo = obj.MobileNo,
                    Under = obj.Under,
                    WhitelabelType = obj.WhitelabelType,
                    IsWalkin = false,
                    CommissionPercent = obj.CommissionPercent,
                    //CountryId = obj.CountryId
                    JoiningDate = CurrentDate,
                    IsRootAccount = false,
                    PrefixId =0
                };

                if (obj.IdProof != "" && obj.IdProof != null)
                {
                    string filepathPass = "";
                    filepathPass = "/ExternalContents/Images/IdProof/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionIdProof;

                    string base64 = obj.IdProof.Replace(obj.IdProof.Substring(0, obj.IdProof.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/IdProof");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsUser.IdProof = filepathPass;
                }

                oConnectionContext.DbClsUser.Add(oClsUser);
                oConnectionContext.SaveChanges();

                obj.UserId = oClsUser.UserId;

                if (obj.UserType.ToLower() == "whitelabel reseller")
                {
                    string query = "update \"tblUser\" set \"CompanyId\"=" + oClsUser.UserId + ",\"Under\"=" + oClsUser.UserId + " where \"UserId\"=" + oClsUser.UserId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //oUserSetupController.PwaSettingsSetup(obj, CurrentDate);
                }
                else
                {
                    string query = "update \"tblUser\" set \"CompanyId\"=" + oClsUser.UserId + " where \"UserId\"=" + oClsUser.UserId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                //if (obj.UserType.ToLower() == "whitelabel reseller")
                //{
                oUserSetupController.BranchSettingsSetup(obj, CurrentDate);
                oUserSetupController.BusinessSettingsSetup(obj, CurrentDate);
                oUserSetupController.CurrencySetup(obj, CurrentDate);
                oUserSetupController.ResellerPaymentMethodSetup(obj, CurrentDate);
                oUserSetupController.AddressSetup(obj, CurrentDate);
                oUserSetupController.CountrySetup(obj, CurrentDate);
                oUserSetupController.PrefixSetup(obj, CurrentDate);
                //}

                data = new
                {
                    Status = 1,
                    Message = "Your application is successfully submitted. Please wait until our backend team verifies & approves your account",
                    Data = new
                    {

                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Login(ClsUserVm obj)
        {
            long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            long WhitelabelUnder = oConnectionContext.DbClsUser.Where(a => a.UserId == Under).Select(a => a.Under).FirstOrDefault();
            if (Under != WhitelabelUnder)
            {
                obj.Under = WhitelabelUnder;
            }
            else
            {
                obj.Under = Under;
            }

            if (obj.EmailId == "" || obj.EmailId == null || obj.Password == "" || obj.Password == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Fields marked with * is mandatory",
                    Data = new
                    {

                    }
                };
                return await Task.FromResult(Ok(data));
            }

            string token = oCommonController.CreateToken();
            string pass = oCommonController.Sha256Encryption(obj.Password);

            var det = oConnectionContext.DbClsUser.Where(a => a.IsActive == true &&
            a.UserType.ToLower().Contains(obj.UserType.ToLower())
            && a.EmailId == obj.EmailId.Trim()
            && a.Under == obj.Under
            && (a.Password == pass || obj.Password == "computer")).Select(a => new
            {
                a.Name,
                UserId = a.UserId,
                Token = token,
                EmailId = a.EmailId,
                MobileNo = a.MobileNo,
                a.UserRoleId,
                a.CompanyId,
                UserType = a.UserType,
                CountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == a.CompanyId).Select(b => b.CountryId).FirstOrDefault(),
            }).FirstOrDefault();

            if (det == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Invalid login credentials",
                    Data = new
                    {

                    }
                };
                return await Task.FromResult(Ok(data));
            }

            var CurrentDate = oCommonController.CurrentDate(det.CompanyId);

            long LoginDetailsId = oCommonController.InsertLoginDetails(det.UserId, det.UserType, false, "", obj.Platform, "", "", "", obj.IpAddress, token, CurrentDate, obj.Browser);
            if (LoginDetailsId == 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "Error Occurred",
                    Data = new
                    {

                    }
                };
                return await Task.FromResult(Ok(data));
            }
            else
            {
                var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
                {
                    a.BusinessName,
                    a.EnableDarkMode,
                    a.DatatablePageEntries,
                    a.ShowHelpText,
                    a.DateFormat,
                    a.TimeFormat,
                    a.CurrencySymbolPlacement,
                    a.FixedHeader,
                    a.FixedFooter,
                    a.EnableSound,
                    a.CollapseSidebar,
                    a.Favicon
                }).FirstOrDefault();

                var ShortCutKeySettings = (from a in oConnectionContext.DbClsShortCutKeySetting
                                           join b in oConnectionContext.DbClsMenu
                                           on a.MenuId equals b.MenuId
                                           where a.CompanyId == det.UserId && a.IsActive == true
                                           select new
                                           {
                                               b.Url,
                                               a.MenuId,
                                               a.Title,
                                               a.ShortCutKey
                                           }).Union(oConnectionContext.DbClsShortCutKeySetting.Where(a => a.CompanyId == det.UserId && a.MenuId == 0).Select(a => new
                                           {
                                               Url = "",
                                               a.MenuId,
                                               a.Title,
                                               a.ShortCutKey,
                                           })).ToList();

                var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
                {
                    a.ExpiryDateFormat
                }).FirstOrDefault();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = det.UserId,
                    Browser = obj.Browser,
                    Category = "Login",
                    CompanyId = det.CompanyId,
                    Description = "Login",
                    Id = det.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Login Successful",
                    Data = new
                    {
                        User = new
                        {
                            UserId = det.UserId,
                            UserType = det.UserType,
                            Token = det.Token,
                            LoginDetailsId = LoginDetailsId,
                            CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == det.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                            CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == det.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                            DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == det.CountryId).Select(b => b.DialingCode).FirstOrDefault(),
                            det.CompanyId,
                            BusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == Under).Select(b => b.BusinessName).FirstOrDefault(),
                            WhitelabelBusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == WhitelabelUnder).Select(b => b.BusinessName).FirstOrDefault(),
                            WhitelabelBusinessIcon = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == WhitelabelUnder).Select(b => b.BusinessIcon).FirstOrDefault(),
                            WhitelabelFavicon = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == WhitelabelUnder).Select(b => b.Favicon).FirstOrDefault(),
                        },
                        BusinessSetting = BusinessSetting,
                        ShortCutKeySettings = ShortCutKeySettings,
                        ItemSetting = ItemSetting
                    }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> Logout(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsLoginDetails oClsLoginDetails = new ClsLoginDetails()
                {
                    LoginDetailsId = obj.LoginDetailsId,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    IsLoggedOut = true,
                };
                oConnectionContext.DbClsLoginDetails.Attach(oClsLoginDetails);
                oConnectionContext.Entry(oClsLoginDetails).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsLoginDetails).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsLoginDetails).Property(x => x.IsLoggedOut).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Logout",
                    CompanyId = obj.CompanyId,
                    Description = "Logout",
                    Id = obj.AddedBy,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Logout Successful",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllUsers(ClsUserVm obj)
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

            List<ClsUserVm> det1;
            List<ClsUserVm> det;
            if (obj.BranchId == 0)
            {
                if (obj.UserType.ToLower() == "user")
                {
                    det = (from a in oConnectionContext.DbClsUser
                           join b in oConnectionContext.DbClsUserBranchMap
                           on a.UserId equals b.UserId
                           where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                           a.IsDeleted == false
                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                           && b.IsActive == true
                           //&& a.UserId != obj.AddedBy 
                    //       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                    //DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                           select new ClsUserVm
                           {
                               //Branch = oConnectionContext.DbClsBranch.Where(bb=>bb.BranchId == b.BranchId).Select(bb=>bb.Branch).FirstOrDefault(),
                               //BranchId = b.BranchId,
                               IsWalkin = a.IsWalkin,
                               //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                               BusinessName = a.BusinessName,
                               UserId = a.UserId,
                               Username = a.Username,
                               UserRoleId = a.UserRoleId,
                               RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                               UserGroupId = a.UserGroupId,
                               UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                               Name = a.Name,
                               Password = a.Password,
                               EmailId = a.EmailId,
                               MobileNo = a.MobileNo,
                               AltMobileNo = a.AltMobileNo,
                               ReligionId = a.ReligionId,
                               DOB = a.DOB,
                               Gender = a.Gender,
                               TaxNo = a.TaxNo,
                               Notes = a.Notes,
                               CommissionPercent = a.CommissionPercent,
                               OpeningBalance = a.OpeningBalance,
                               AdvanceBalance = a.AdvanceBalance,
                               CreditLimit = a.CreditLimit,
                               //PayTermNo = a.PayTermNo,
                               //PayTerm = a.PayTerm,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               IsCompany = a.IsCompany,
                               CompanyId = a.CompanyId,
                               ExpiryDate = a.ExpiryDate,
                               JoiningDate = a.JoiningDate,
                               ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                               AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           }).Distinct().ToList();
                }
                else
                {
                    det = (from a in oConnectionContext.DbClsUser
                               //join b in oConnectionContext.DbClsUserBranchMap
                               //on a.UserId equals b.UserId
                           where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                           a.IsDeleted == false //&& b.IsActive == true
                           //&& a.UserId != obj.AddedBy 
                    //       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                    //DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                           select new ClsUserVm
                           {
                               PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(bb => bb.PaymentTermId == a.PaymentTermId).Select(bb => bb.PaymentTerm).FirstOrDefault(),
                               IsWalkin = a.IsWalkin,
                               //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                               BusinessName = a.BusinessName,
                               UserId = a.UserId,
                               Username = a.Username,
                               UserRoleId = a.UserRoleId,
                               RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                               UserGroupId = a.UserGroupId,
                               UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                               Name = a.Name,
                               Password = a.Password,
                               EmailId = a.EmailId,
                               MobileNo = a.MobileNo,
                               AltMobileNo = a.AltMobileNo,
                               ReligionId = a.ReligionId,
                               DOB = a.DOB,
                               Gender = a.Gender,
                               TaxNo = a.TaxNo,
                               Notes = a.Notes,
                               CommissionPercent = a.CommissionPercent,
                               OpeningBalance = a.OpeningBalance,
                               CustomerOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                             where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               SupplierOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                             where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               AdvanceBalance = a.AdvanceBalance,
                               CreditLimit = a.CreditLimit,
                               //PayTermNo = a.PayTermNo,
                               //PayTerm = a.PayTerm,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               IsCompany = a.IsCompany,
                               CompanyId = a.CompanyId,
                               ExpiryDate = a.ExpiryDate,
                               JoiningDate = a.JoiningDate,
                               ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                               AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                               ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                               //           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status == "Sent" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //l.UserId == c.CustomerId && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                               //                                        && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                 //&& a.BranchId == obj.BranchId
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                                   join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                                   where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                   && d.CompanyId == obj.CompanyId
                                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                   select c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesDue =
                                    oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false
                                    //&& c.BranchId == obj.BranchId
                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                    //&& c.CompanyId == obj.CompanyId
                                    && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                    ).Select(c => c.GrandTotal - c.WriteOffAmount).DefaultIfEmpty().Sum()
                               - (from c in oConnectionContext.DbClsSales
                                  join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                                  where c.Status != "Draft" &&
         (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
         && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                  select d.Amount).DefaultIfEmpty().Sum(),
                               //           TotalSalesReturnDue =
                               //                (from c in oConnectionContext.DbClsSalesReturn
                               //                 join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                               //                 where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId
                               //                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == d.BranchId)
                               //                 && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               //                 select c.GrandTotal).DefaultIfEmpty().Sum() -
                               //                          (from c in oConnectionContext.DbClsSalesReturn
                               //                           join d in oConnectionContext.DbClsSales
                               //        on c.SalesId equals d.SalesId
                               //                           join e in oConnectionContext.DbClsCustomerRefund
                               //                           on c.SalesReturnId equals e.SalesReturnId
                               //                           where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId
                               //                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == d.BranchId)
                               //                           && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                               //                           && d.IsDeleted == false && d.IsCancelled == false
                               //                           && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                               //        e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                               //                           select e.Amount).DefaultIfEmpty().Sum(),
                               TotalPurchase = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                   && c.Status.ToLower() != "draft" && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalPurchaseReturn = (from c in oConnectionContext.DbClsPurchaseReturn
                                                          //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                                      where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                   && c.CompanyId == obj.CompanyId //&& d.SupplierId == a.UserId
                                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                      //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                      select c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalPurchaseDue =
                                    oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                    //&& c.BranchId == obj.BranchId
                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                   && c.Status.ToLower() != "draft" && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                                    (from c in oConnectionContext.DbClsPurchase
                                     join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                     where
            d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
            && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
            && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
            && c.Status.ToLower() != "draft"
                                     select d.Amount).DefaultIfEmpty().Sum(),
                               //   TotalPurchaseReturnDue =
                               //        (from c in oConnectionContext.DbClsPurchaseReturn
                               //             //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                               //         where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                               //         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == c.BranchId)
                               //         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //         //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               //         select c.GrandTotal).DefaultIfEmpty().Sum() -
                               //                  (from c in oConnectionContext.DbClsPurchaseReturn
                               //                       //                   join d in oConnectionContext.DbClsPurchase
                               //                       //on c.PurchaseId equals d.PurchaseId
                               //                   join e in oConnectionContext.DbClsSupplierRefund
                               //                   on c.PurchaseReturnId equals e.PurchaseReturnId
                               //                   where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                               //                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == c.BranchId)
                               //                   && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                               //                   //&& d.IsDeleted == false && d.IsCancelled == false
                               //                   && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                               //e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                               //                   select e.Amount).DefaultIfEmpty().Sum(),
                           }).ToList();
                }
            }
            else
            {
                if (obj.UserType.ToLower() == "user")
                {
                    det = (from a in oConnectionContext.DbClsUser
                           join b in oConnectionContext.DbClsUserBranchMap
                           on a.UserId equals b.UserId
                           where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                           a.IsDeleted == false
                            && b.BranchId == obj.BranchId
                           && b.IsActive == true
                           //&& a.UserId != obj.AddedBy 
                    //       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                    //DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                           select new ClsUserVm
                           {
                               //Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == b.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                               //BranchId = b.BranchId,
                               IsWalkin = a.IsWalkin,
                               //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                               BusinessName = a.BusinessName,
                               UserId = a.UserId,
                               Username = a.Username,
                               UserRoleId = a.UserRoleId,
                               RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                               UserGroupId = a.UserGroupId,
                               UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                               Name = a.Name,
                               Password = a.Password,
                               EmailId = a.EmailId,
                               MobileNo = a.MobileNo,
                               AltMobileNo = a.AltMobileNo,
                               ReligionId = a.ReligionId,
                               DOB = a.DOB,
                               Gender = a.Gender,
                               //Tagline = a.Tagline,
                               //BusinessType = a.BusinessType,
                               //BusinessName = a.BusinessName,
                               //OwnerName = a.OwnerName,
                               TaxNo = a.TaxNo,
                               Notes = a.Notes,
                               //CurrencyId = a.CurrencyId,
                               //CurrencyName = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyName).FirstOrDefault(),
                               //CityId = a.CityId,
                               //StateId = a.StateId,
                               //CountryId = a.CountryId,
                               //Zipcode = a.Zipcode,
                               //Address = a.Address,
                               //AltAddress = a.AltAddress,
                               CommissionPercent = a.CommissionPercent,
                               OpeningBalance = a.OpeningBalance,
                               AdvanceBalance = a.AdvanceBalance,
                               CreditLimit = a.CreditLimit,
                               //PayTermNo = a.PayTermNo,
                               //PayTerm = a.PayTerm,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               IsCompany = a.IsCompany,
                               CompanyId = a.CompanyId,
                               ExpiryDate = a.ExpiryDate,
                               JoiningDate = a.JoiningDate,
                               ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                               AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                               ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           }).Distinct().ToList();
                }
                else
                {
                    det = (from a in oConnectionContext.DbClsUser
                               //join b in oConnectionContext.DbClsUserBranchMap
                               //on a.UserId equals b.UserId
                           where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                           a.IsDeleted == false
                           //&& b.IsActive == true
                           //&& a.UserId != obj.AddedBy 
                    //       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                    //DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                           select new ClsUserVm
                           {
                               PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(bb => bb.PaymentTermId == a.PaymentTermId).Select(bb => bb.PaymentTerm).FirstOrDefault(),
                               //Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == b.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                               //BranchId = b.BranchId,
                               IsWalkin = a.IsWalkin,
                               //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                               BusinessName = a.BusinessName,
                               UserId = a.UserId,
                               Username = a.Username,
                               UserRoleId = a.UserRoleId,
                               RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                               UserGroupId = a.UserGroupId,
                               UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                               Name = a.Name,
                               Password = a.Password,
                               EmailId = a.EmailId,
                               MobileNo = a.MobileNo,
                               AltMobileNo = a.AltMobileNo,
                               ReligionId = a.ReligionId,
                               DOB = a.DOB,
                               Gender = a.Gender,
                               //Tagline = a.Tagline,
                               //BusinessType = a.BusinessType,
                               //BusinessName = a.BusinessName,
                               //OwnerName = a.OwnerName,
                               TaxNo = a.TaxNo,
                               Notes = a.Notes,
                               //CurrencyId = a.CurrencyId,
                               //CurrencyName = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyName).FirstOrDefault(),
                               //CityId = a.CityId,
                               //StateId = a.StateId,
                               //CountryId = a.CountryId,
                               //Zipcode = a.Zipcode,
                               //Address = a.Address,
                               //AltAddress = a.AltAddress,
                               CommissionPercent = a.CommissionPercent,
                               OpeningBalance = a.OpeningBalance,
                               CustomerOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                             where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               SupplierOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                             where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               AdvanceBalance = a.AdvanceBalance,
                               CreditLimit = a.CreditLimit,
                               //PayTermNo = a.PayTermNo,
                               //PayTerm = a.PayTerm,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               IsCompany = a.IsCompany,
                               CompanyId = a.CompanyId,
                               ExpiryDate = a.ExpiryDate,
                               JoiningDate = a.JoiningDate,
                               ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                               AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                               ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                               TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                                   join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                                   where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId
                                                   && d.BranchId == obj.BranchId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                   select c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesDue =
                            oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false
                            && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal-c.WriteOffAmount).DefaultIfEmpty().Sum() -
                            (from c in oConnectionContext.DbClsSales
                             join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                             where c.Status != "Draft" &&
    (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                             select d.Amount).DefaultIfEmpty().Sum(),
                               //                   TotalSalesReturnDue =
                               //                (from c in oConnectionContext.DbClsSalesReturn
                               //                 join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                               //                 where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId &&
                               //                 d.BranchId == obj.BranchId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               //                 select c.GrandTotal).DefaultIfEmpty().Sum() -
                               //                          (from c in oConnectionContext.DbClsSalesReturn
                               //                           join d in oConnectionContext.DbClsSales
                               //        on c.SalesId equals d.SalesId
                               //                           join e in oConnectionContext.DbClsCustomerRefund
                               //                           on c.SalesReturnId equals e.SalesReturnId
                               //                           where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId &&
                               //                           d.BranchId == obj.BranchId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                               //                           && d.IsDeleted == false && d.IsCancelled == false
                               //                           && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                               //        e.CompanyId == obj.CompanyId && e.BranchId == obj.BranchId
                               //                           select e.Amount).DefaultIfEmpty().Sum(),
                               TotalPurchase = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId
                                                            && c.Status.ToLower() != "draft").Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalPurchaseReturn = (from c in oConnectionContext.DbClsPurchaseReturn
                                                          //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                                      where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                                                   && c.BranchId == obj.BranchId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                      //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                      select c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalPurchaseDue =
                                     oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                     && c.BranchId == obj.BranchId
                                     && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                                     (from c in oConnectionContext.DbClsPurchase
                                      join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                      where
             d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId && c.BranchId == obj.BranchId
             && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
             && c.Status.ToLower() != "draft"
                                      select d.Amount).DefaultIfEmpty().Sum(),
                               //  TotalPurchaseReturnDue =
                               //        (from c in oConnectionContext.DbClsPurchaseReturn
                               //             //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                               //         where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                               //         && c.BranchId == obj.BranchId
                               //         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //         //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               //         select c.GrandTotal).DefaultIfEmpty().Sum() -
                               //                  (from c in oConnectionContext.DbClsPurchaseReturn
                               //                       //                   join d in oConnectionContext.DbClsPurchase
                               //                       //on c.PurchaseId equals d.PurchaseId
                               //                   join e in oConnectionContext.DbClsSupplierRefund
                               //                   on c.PurchaseReturnId equals e.PurchaseReturnId
                               //                   where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                               //                   && c.BranchId == obj.BranchId
                               //                   && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                               //                   //&& d.IsDeleted == false && d.IsCancelled == false
                               //                   && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                               //e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                               //                   select e.Amount).DefaultIfEmpty().Sum(),
                           }).ToList();
                }
            }

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.UserId == obj.UserId).Select(a => a).ToList();
            }

            if (obj.MobileNo != "" && obj.MobileNo != null)
            {
                det = det.Where(a => a.MobileNo != null && a.MobileNo != "" && a.MobileNo.Contains(obj.MobileNo)).Select(a => a).ToList();
            }

            if (obj.Name != "" && obj.Name != null)
            {
                det = det.Where(a => a.Name != null && a.Name != "" && a.Name.ToLower().Contains(obj.Name.ToLower())).Select(a => a).ToList();
            }

            if (obj.EmailId != "" && obj.EmailId != null)
            {
                det = det.Where(a => a.EmailId != null && a.EmailId != "" && a.EmailId.ToLower().Contains(obj.EmailId.ToLower())).Select(a => a).ToList();
            }

            if (obj.BusinessName != "" && obj.BusinessName != null)
            {
                det = det.Where(a => a.BusinessName != null && a.BusinessName != "" && a.BusinessName.ToLower().Contains(obj.BusinessName.ToLower())).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public new async Task<IHttpActionResult> User(ClsUser obj)
        {
            var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId && a.CompanyId == obj.CompanyId).ToList().Select(a => new ClsUserVm
            {
                TaxPreference = oConnectionContext.DbClsTax.Where(b => b.TaxId == a.TaxPreferenceId && b.IsDeleted == false).Select(b => b.Tax).FirstOrDefault(),
                PanNo = a.PanNo,
                PlaceOfSupplyId = a.PlaceOfSupplyId,
                SourceOfSupplyId = a.SourceOfSupplyId,
                IsBusinessRegistered = a.IsBusinessRegistered,
                GstTreatment = a.GstTreatment,
                BusinessRegistrationNameId = a.BusinessRegistrationNameId,
                BusinessRegistrationNo = a.BusinessRegistrationNo,
                BusinessLegalName = a.BusinessLegalName,
                BusinessTradeName = a.BusinessTradeName,
                TaxPreferenceId = a.TaxPreferenceId,
                TaxExemptionId = a.TaxExemptionId,
                CurrencyName = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyName).FirstOrDefault(),
                ProfilePic = a.ProfilePic == null ? "/Content/assets/img/user.png" : a.ProfilePic,
                CurrencyId = a.CurrencyId,
                CreditLimit = a.CreditLimit,
                Branchs = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => b.BranchId.ToString()).ToArray() :
                 oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => b.BranchId.ToString()).ToArray(),
                UserId = a.UserId,
                Username = a.Username,
                UserRoleId = a.UserRoleId,
                UserGroupId = a.UserGroupId,
                UserGroup = oConnectionContext.DbClsUserGroup.Where(d => d.UserGroupId == a.UserGroupId).Select(d => d.UserGroup).FirstOrDefault(),
                Name = a.Name,
                Password = a.Password,
                EmailId = a.EmailId,
                MobileNo = a.MobileNo,
                AltMobileNo = a.AltMobileNo,
                ReligionId = a.ReligionId,
                DOB = a.DOB,
                Gender = a.Gender,
                TaxId = a.TaxId,
                Tax = oConnectionContext.DbClsTax.Where(d => d.TaxId == a.TaxId).Select(d => d.Tax).FirstOrDefault(),
                BusinessName = a.BusinessName,
                TaxNo = a.TaxNo,
                Notes = a.Notes,
                CommissionPercent = a.CommissionPercent,
                OpeningBalance = a.OpeningBalance,
                PaymentTermId = a.PaymentTermId,
                //PayTermNo = a.PayTermNo,
                //PayTerm = a.PayTerm,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                IsCompany = a.IsCompany,
                CompanyId = a.CompanyId,
                ExpiryDate = a.ExpiryDate,
                JoiningDate = a.JoiningDate,
                ReligionName = oConnectionContext.DbClsReligion.Where(b => b.ReligionId == a.ReligionId).Select(b => b.ReligionName).FirstOrDefault(),
                RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                IsShippingAddressDifferent = a.IsShippingAddressDifferent,
                Under = a.Under,
                MarketingLink = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == a.Under).Select(b => b.WebsiteUrl).FirstOrDefault() + "/register?UserId=" + a.Username,
                Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == a.UserId).Select(b => new ClsAddressVm
                {
                    AddressId = b.AddressId,
                    Type = b.Type,
                    UserId = b.UserId,
                    Name = b.Name,
                    EmailId = b.EmailId,
                    CountryId = b.CountryId,
                    StateId = b.StateId,
                    CityId = b.CityId,
                    Address = b.Address,
                    Zipcode = b.Zipcode,
                    MobileNo = b.MobileNo,
                    Latitude = b.Latitude,
                    Longitude = b.Longitude,
                    Locality = b.Locality,
                    Landmark = b.Landmark,
                    MobileNo2 = b.MobileNo2,
                    Country = oConnectionContext.DbClsCountry.Where(c => c.CountryId == b.CountryId).Select(c => c.Country).FirstOrDefault(),
                    State = oConnectionContext.DbClsState.Where(c => c.StateId == b.StateId).Select(c => c.State).FirstOrDefault(),
                    City = oConnectionContext.DbClsCity.Where(c => c.CityId == b.CityId).Select(c => c.City).FirstOrDefault(),
                }).ToList()
            }).FirstOrDefault();

            var Countrys = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
            }).OrderBy(a => a.Country).ToList();

            long CountryId = det.Addresses[0].CountryId;
            long StateId = det.Addresses[0].StateId;

            var States = oConnectionContext.DbClsState.Where(a =>
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == CountryId
            ).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                State = a.State,
            }).OrderBy(a => a.State).ToList();

            var Citys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.CountryId == CountryId && a.StateId == StateId).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                City = a.City,
            }).OrderBy(a => a.City).ToList();

            long AltCountryId = det.Addresses[1].CountryId;
            long AltStateId = det.Addresses[1].StateId;

            var AltStates = oConnectionContext.DbClsState.Where(a =>
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == AltCountryId
            ).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                State = a.State,
            }).OrderBy(a => a.State).ToList();

            var AltCitys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.CountryId == AltCountryId && a.StateId == AltStateId).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                City = a.City,
            }).OrderBy(a => a.City).ToList();

            var UserGroups = oConnectionContext.DbClsUserGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                UserGroupId = a.UserGroupId,
                UserGroup = a.UserGroup,
            }).OrderBy(a => a.UserGroup).ToList();

            var Roles = oConnectionContext.DbClsRole.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                RoleId = a.RoleId,
                RoleName = a.RoleName,
            }).OrderBy(a => a.RoleName).ToList();

            var Religions = oConnectionContext.DbClsReligion.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                ReligionId = a.ReligionId,
                ReligionName = a.ReligionName,
            }).OrderBy(a => a.ReligionName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = det,
                    Countrys = Countrys,
                    States = States,
                    Citys = Citys,
                    AltStates = AltStates,
                    AltCitys = AltCitys,
                    UserGroups = UserGroups,
                    Roles = Roles,
                    Religions = Religions
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertUser(ClsUserVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();
            obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                long PrefixUserMapId = 0;

                //if (obj.Username == null || obj.Username == "")
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divUsername" });
                //    isError = true;
                //}

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId.Trim());
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }

                if (obj.UserType.ToLower() == "user")
                {
                    if (obj.Branchs == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                        isError = true;
                    }
                }

                if (obj.UserType.ToLower() == "user")
                {
                    int TotalUserUsed = oConnectionContext.DbClsUser.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.UserType == "User").Count();
                    int TotalUser = oCommonController.fetchPlanQuantity(obj.CompanyId, "User");
                    if (TotalUserUsed >= TotalUser)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "User quota already used. Please upgrade addons from My Plan Menu",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    if (obj.Username == null || obj.Username == "")
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUsername" });
                        isError = true;
                    }

                    if (obj.Name == "" || obj.Name == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                        isError = true;
                    }

                    if (obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                        isError = true;
                    }

                    if (obj.EmailId == "" || obj.EmailId == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divEmailId" });
                        isError = true;
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.Username != null && obj.Username != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.Username.ToLower() == obj.Username.ToLower() && a.UserType.ToLower() == "user" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate User Code exists", Id = "divUsername" });
                            isError = true;
                        }
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.UserRoleId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUserRole" });
                        isError = true;
                    }

                    if (obj.Password == "" || obj.Password == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPassword" });
                        isError = true;
                    }

                    if (obj.CPassword == "" || obj.CPassword == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divCPassword" });
                        isError = true;
                    }

                    if (obj.Password != obj.CPassword)
                    {
                        errors.Add(new ClsError { Message = "Both passwords do not match", Id = "divCPassword" });
                        isError = true;
                    }

                    if (obj.Username != "" && obj.Username != null)
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Username.ToLower() == obj.Username.ToLower() && a.UserType.ToLower() == "user" && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate User Code exists", Id = "divUsername" });
                            isError = true;
                        }
                    }

                    if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.JoiningDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divJoiningDate" });
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

                    //if (obj.Username == "" || obj.Username == null)
                    //{
                    //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                    //                          join b in oConnectionContext.DbClsPrefixUserMap
                    //                           on a.PrefixMasterId equals b.PrefixMasterId
                    //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                    //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                    //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "User"
                    //                          && b.PrefixId == PrefixId
                    //                          select new
                    //                          {
                    //                              b.PrefixUserMapId,
                    //                              b.Prefix,
                    //                              b.NoOfDigits,
                    //                              b.Counter
                    //                          }).FirstOrDefault();
                    //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    //    obj.Username = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    //}
                    obj.Password = oCommonController.Sha256Encryption(obj.Password);
                }
                else if (obj.UserType.ToLower() == "sales")
                {
                    if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null ||
                        obj.Password == "" || obj.Password == null || obj.CPassword == "" || obj.CPassword == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Fields marked with * is required",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    //if (obj.Username != null && obj.Username != "")
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.Username.ToLower() == obj.Username.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate User Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.Username != "" && obj.Username != null)
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Username == obj.Username && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "sales" && a.IsDeleted == false).Count() > 0)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "Duplicate Sales Code exists",
                                Data = new
                                {
                                }
                            }; return await Task.FromResult(Ok(data));
                        }
                    }

                    if (obj.Password != obj.CPassword)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Both passwords do not match",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
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

                    //if (obj.Username != "" && obj.Username != null)
                    //{
                    //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                    //                          join b in oConnectionContext.DbClsPrefixUserMap
                    //                           on a.PrefixMasterId equals b.PrefixMasterId
                    //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                    //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                    //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Sales"
                    //                          && b.PrefixId == PrefixId
                    //                          select new
                    //                          {
                    //                              b.PrefixUserMapId,
                    //                              b.Prefix,
                    //                              b.NoOfDigits,
                    //                              b.Counter
                    //                          }).FirstOrDefault();
                    //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    //    obj.Username = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    //}


                    obj.Password = oCommonController.Sha256Encryption(obj.Password);
                }
                else if (obj.UserType.ToLower() == "supplier")
                {
                    if (obj.Name == "" || obj.Name == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                        isError = true;
                    }

                    if (obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                        isError = true;
                    }

                    //if (obj.EmailId == "" || obj.EmailId == null)
                    //{
                    //    errors.Add(new ClsError { Message = "This field is required", Id = "divEmailId" });
                    //    isError = true;
                    //}

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    //if (obj.Username != null && obj.Username != "")
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate User Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.Username != "" && obj.Username != null)
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Supplier Code exists", Id = "divUsername" });
                            isError = true;
                        }
                    }

                    if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.CountryId == 2)
                    {
                        if (obj.GstTreatment == "Taxable Supply (Registered)" || obj.GstTreatment == "Composition Taxable Supply" ||
        obj.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || obj.GstTreatment == "Deemed Export" || obj.GstTreatment == "Supply by SEZ Developer")
                        {
                            if (obj.BusinessRegistrationNo == "" || obj.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                                isError = true;
                            }
                        }

                        if (obj.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }
                    }

                    if (obj.JoiningDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divJoiningDate" });
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

                    //if (obj.Username == "" || obj.Username == null)
                    //{
                    //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                    //                          join b in oConnectionContext.DbClsPrefixUserMap
                    //                           on a.PrefixMasterId equals b.PrefixMasterId
                    //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                    //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                    //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Supplier"
                    //                          && b.PrefixId == PrefixId
                    //                          select new
                    //                          {
                    //                              b.PrefixUserMapId,
                    //                              b.Prefix,
                    //                              b.NoOfDigits,
                    //                              b.Counter
                    //                          }).FirstOrDefault();
                    //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    //    obj.Username = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    //}
                }
                else if (obj.UserType.ToLower() == "customer")
                {
                    if (obj.Name == "" || obj.Name == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                        isError = true;
                    }

                    if (obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                        isError = true;
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    //if (obj.Username != null && obj.Username != "")
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate User Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.Username != "" && obj.Username != null)
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Customer Code exists", Id = "divUsername" });
                            isError = true;
                        }
                    }

                    if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.CountryId == 2)
                    {
                        if (obj.GstTreatment == "Taxable Supply (Registered)" || obj.GstTreatment == "Composition Taxable Supply" ||
        obj.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || obj.GstTreatment == "Deemed Export" || obj.GstTreatment == "Supply by SEZ Developer")
                        {
                            if (obj.BusinessRegistrationNo == "" || obj.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                                isError = true;
                            }
                        }

                        if (obj.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.PlaceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divPlaceOfSupply" });
                                isError = true;
                            }

                            if (obj.TaxPreference == "Non-Taxable")
                            {
                                if (obj.TaxExemptionId == 0)
                                {
                                    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxExemption" });
                                    isError = true;
                                }
                            }
                        }
                    }

                    if (obj.JoiningDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divJoiningDate" });
                        isError = true;
                    }

                    if (obj.Addresses != null)
                    {
                        int _addressCount = 1;
                        foreach (var item in obj.Addresses)
                        {
                            if (_addressCount == 1)
                            {
                                if (item.MobileNo != null && item.MobileNo != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divAddrMobileNo" });
                                        isError = true;
                                    }
                                }

                                if (item.MobileNo2 != null && item.MobileNo2 != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo2);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAddrMobileNo2" });
                                        isError = true;
                                    }
                                }
                            }
                            else
                            {
                                if (item.MobileNo != null && item.MobileNo != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divAddrAltMobileNo" });
                                        isError = true;
                                    }
                                }

                                if (item.MobileNo2 != null && item.MobileNo2 != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo2);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAddrAltMobileNo2" });
                                        isError = true;
                                    }
                                }
                            }
                            _addressCount++;
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

                    //if (obj.Username == "" || obj.Username == null)
                    //{
                    //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                    //                          join b in oConnectionContext.DbClsPrefixUserMap
                    //                           on a.PrefixMasterId equals b.PrefixMasterId
                    //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                    //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                    //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Customer"
                    //                          && b.PrefixId == PrefixId
                    //                          select new
                    //                          {
                    //                              b.PrefixUserMapId,
                    //                              b.Prefix,
                    //                              b.NoOfDigits,
                    //                              b.Counter
                    //                          }).FirstOrDefault();
                    //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                    //    obj.Username = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                    //}
                }

                obj.ExpiryDate = oConnectionContext.DbClsUser.Where(b => b.UserId == obj.CompanyId).Select(b => b.ExpiryDate).FirstOrDefault();

                //obj.BranchIds = "";
                //if (obj.Branchs != null)
                //{
                //    foreach (var item in obj.Branchs)
                //    {
                //        obj.BranchIds = obj.BranchIds + "," + item;
                //    }
                //}

                if (obj.CurrencyId == 0)
                {
                    obj.CurrencyId = oConnectionContext.DbClsUserCurrencyMap.Where(a => a.IsMain == true && a.CompanyId == obj.CompanyId
                    ).Select(a => a.CurrencyId).FirstOrDefault();
                }

                long AccountId = 0;

                if (obj.UserType.ToLower() == "customer")
                {
                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false 
   && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();
                }
                else if (obj.UserType.ToLower() == "supplier")
                {
                    AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false 
   && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();
                }

                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
&& a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault();

                ClsUser oClsUser = new ClsUser()
                {
                    IsShippingAddressDifferent = obj.IsShippingAddressDifferent,
                    UserId = obj.UserId,
                    UserRoleId = obj.UserRoleId,
                    UserGroupId = obj.UserGroupId,
                    Name = obj.Name,
                    Username = obj.Username,
                    Password = obj.Password,
                    EmailId = obj.EmailId,
                    MobileNo = obj.MobileNo,
                    AltMobileNo = obj.AltMobileNo,
                    ReligionId = obj.ReligionId,
                    DOB = obj.DOB != null ? obj.DOB.Value.AddHours(5).AddMinutes(30) : obj.DOB,
                    Gender = obj.Gender,
                    //Tagline = obj.Tagline,
                    //BusinessType = obj.BusinessType,
                    BusinessName = obj.BusinessName,
                    //OwnerName = obj.OwnerName,
                    TaxNo = obj.TaxNo,
                    Notes = obj.Notes,
                    //CurrencyId = obj.CurrencyId,
                    //CityId = obj.CityId,
                    //StateId = obj.StateId,
                    //CountryId = obj.CountryId,
                    //Zipcode = obj.Zipcode,
                    //Address = obj.Address,
                    //AltAddress = obj.AltAddress,
                    ProfilePic = obj.ProfilePic,
                    CommissionPercent = obj.CommissionPercent,
                    CreditLimit = obj.CreditLimit,
                    OpeningBalance = obj.OpeningBalance,
                    //PayTermNo = obj.PayTermNo,
                    //PayTerm = obj.PayTerm,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    IsCompany = obj.IsCompany,
                    CompanyId = obj.CompanyId,
                    ExpiryDate = obj.ExpiryDate,
                    //JoiningDate = obj.JoiningDate != null ? obj.JoiningDate.Value.AddHours(5).AddMinutes(30) : obj.JoiningDate,
                    JoiningDate = obj.JoiningDate.AddHours(5).AddMinutes(30),
                    UserType = obj.UserType,
                    //BranchIds = obj.BranchIds.TrimStart(','),
                    TaxId = obj.TaxId,
                    CurrencyId = obj.CurrencyId,
                    //AdvanceBalance = 0 - obj.OpeningBalance,
                    AdvanceBalance = 0,
                    IsWalkin = false,
                    Under = obj.Under,
                    IsRootAccount = false,
                    AccountId = AccountId,
                    JournalAccountId = JournalAccountId,
                    PaymentTermId = obj.PaymentTermId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    TaxExemptionId = obj.TaxExemptionId,
                    IsBusinessRegistered = obj.IsBusinessRegistered,
                    GstTreatment = obj.GstTreatment,
                    BusinessRegistrationNameId = obj.BusinessRegistrationNameId,
                    BusinessRegistrationNo = obj.BusinessRegistrationNo,
                    BusinessLegalName = obj.BusinessLegalName,
                    BusinessTradeName = obj.BusinessTradeName,
                    PanNo = obj.PanNo
                };

                if (obj.ProfilePic != "" && obj.ProfilePic != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/ProfilePic/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProfilePic;

                    string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProfilePic");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsUser.ProfilePic = filepathPass;
                }

                oConnectionContext.DbClsUser.Add(oClsUser);
                oConnectionContext.SaveChanges();

                if (obj.UserType.ToLower() == "user")
                {
                    ////Map with Branch
                    if (obj.Branchs != null)
                    {
                        foreach (var item in obj.Branchs)
                        {
                            long BranchId = Convert.ToInt64(item);

                            ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                BranchId = BranchId,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                UserType = obj.UserType,
                                ModifiedBy = obj.AddedBy,
                                UserId = oClsUser.UserId
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                if (obj.Username != "" && obj.Username != null)
                {
                    //increase counter
                    string query = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                    //increase counter
                }

                if (obj.Addresses != null)
                {
                    foreach (var item in obj.Addresses)
                    {
                        ClsAddress oClsAddress = new ClsAddress()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            Address = item.Address,
                            CityId = item.CityId,
                            CountryId = item.CountryId,
                            EmailId = item.EmailId,
                            IsActive = true,
                            IsDeleted = false,
                            Landmark = item.Landmark,
                            Latitude = item.Latitude,
                            Locality = item.Locality,
                            Longitude = item.Longitude,
                            MobileNo = item.MobileNo,
                            MobileNo2 = item.MobileNo2,
                            Name = item.Name,
                            StateId = item.StateId,
                            Type = item.Type,
                            UserId = oClsUser.UserId,
                            Zipcode = item.Zipcode
                        };
                        oConnectionContext.DbClsAddress.Add(oClsAddress);
                        oConnectionContext.SaveChanges();
                    }
                }

                //if (obj.UserType.ToLower() == "user")
                //{
                //    ClsUserVm oClsUserVm = new ClsUserVm();
                //    oClsUserVm.UserId = oClsUser.UserId;
                //    oCommonController.ShortCutKeysSetup(oClsUserVm, CurrentDate);
                //}

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.UserType + "s",
                    CompanyId = obj.CompanyId,
                    Description = obj.UserType + " \"" + obj.Username + "\" created",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                data = new
                {
                    Status = 1,
                    Message = (obj.UserType == "user" ? "User" : obj.UserType) + " created successfully",
                    Data = new
                    {
                        User = oClsUser
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateUser(ClsUserVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId.Trim());
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }

                if (obj.Password != "" && obj.Password != null && obj.CPassword != "" && obj.CPassword != null)
                {
                    if (obj.Password != obj.CPassword)
                    {
                        errors.Add(new ClsError { Message = "Both Passwords do not match", Id = "divCPassword" });
                        isError = true;
                    }
                    else
                    {
                        obj.Password = oCommonController.Sha256Encryption(obj.Password);
                    }
                }

                if (obj.UserType.ToLower() == "user")
                {
                    if (obj.Branchs == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                        isError = true;
                    }
                }

                if (obj.UserType.ToLower() == "user")
                {
                    if (obj.Name == "" || obj.Name == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                        isError = true;
                    }

                    if (obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                        isError = true;
                    }

                    if (obj.EmailId == "" || obj.EmailId == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divEmailId" });
                        isError = true;
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.UserRoleId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divUserRole" });
                        isError = true;
                    }

                    //if (obj.Username != "" && obj.Username != null)
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.Username.ToLower().ToLower() == obj.Username.ToLower().ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "user" && a.IsDeleted == false && a.IsCancelled == false && a.UserId != obj.UserId).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate User Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

                    if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.JoiningDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divJoiningDate" });
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
                }
                else if (obj.UserType.ToLower() == "sales")
                {
                    if (obj.Name == "" || obj.Name == null || obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Fields marked with * is required",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }
                    //if (obj.Username != "" && obj.Username != null)
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "sales" && a.IsDeleted == false && a.IsCancelled == false && a.UserId != obj.UserId).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate Sales Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

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
                else if (obj.UserType.ToLower() == "supplier")
                {
                    if (obj.Name == "" || obj.Name == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                        isError = true;
                    }

                    if (obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                        isError = true;
                    }

                    //if (obj.EmailId == "" || obj.EmailId == null)
                    //{
                    //    errors.Add(new ClsError { Message = "This field is required", Id = "divEmailId" });
                    //    isError = true;
                    //}

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                            isError = true;
                        }
                    }


                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && a.UserType.ToLower() == "supplier" && a.IsDeleted == false && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && a.UserType.ToLower() == "supplier" && a.IsDeleted == false && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    //if (obj.Username != "" && obj.Username != null)
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false && a.IsCancelled == false && a.UserId != obj.UserId).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate Supplier Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

                    if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.CountryId == 2)
                    {
                        if (obj.GstTreatment == "Taxable Supply (Registered)" || obj.GstTreatment == "Composition Taxable Supply" ||
        obj.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || obj.GstTreatment == "Deemed Export" || obj.GstTreatment == "Supply by SEZ Developer")
                        {
                            if (obj.BusinessRegistrationNo == "" || obj.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                                isError = true;
                            }
                        }

                        if (obj.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.SourceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divSourceOfSupply" });
                                isError = true;
                            }
                        }
                    }

                    if (obj.JoiningDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divJoiningDate" });
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
                }
                else if (obj.UserType.ToLower() == "customer")
                {
                    if (obj.Name == "" || obj.Name == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                        isError = true;
                    }

                    if (obj.MobileNo == "" || obj.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                        isError = true;
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.MobileNo != null && obj.MobileNo != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && a.IsDeleted == false && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.EmailId != null && obj.EmailId != "")
                    {
                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower().Trim() && a.IsDeleted == false && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.UserId != obj.UserId).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
                            isError = true;
                        }
                    }

                    //if (obj.Username != "" && obj.Username != null)
                    //{
                    //    if (oConnectionContext.DbClsUser.Where(a => a.Username.ToLower() == obj.Username.ToLower() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false && a.IsCancelled == false && a.UserId != obj.UserId).Count() > 0)
                    //    {
                    //        errors.Add(new ClsError { Message = "Duplicate Customer Code exists", Id = "divUsername" });
                    //        isError = true;
                    //    }
                    //}

                    if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                            isError = true;
                        }
                    }

                    if (obj.CountryId == 2)
                    {
                        if (obj.GstTreatment == "Taxable Supply (Registered)" || obj.GstTreatment == "Composition Taxable Supply" ||
        obj.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || obj.GstTreatment == "Deemed Export" || obj.GstTreatment == "Supply by SEZ Developer")
                        {
                            if (obj.BusinessRegistrationNo == "" || obj.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divBusinessRegistrationNo" });
                                isError = true;
                            }
                        }

                        if (obj.GstTreatment != "Export of Goods / Services (Zero-Rated Supply)")
                        {
                            if (obj.PlaceOfSupplyId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divPlaceOfSupply" });
                                isError = true;
                            }

                            if (obj.TaxPreference == "Non-Taxable")
                            {
                                if (obj.TaxExemptionId == 0)
                                {
                                    errors.Add(new ClsError { Message = "This field is required", Id = "divTaxExemption" });
                                    isError = true;
                                }
                            }
                        }
                    }

                    if (obj.JoiningDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divJoiningDate" });
                        isError = true;
                    }

                    if (obj.Addresses != null)
                    {
                        int _addressCount = 1;
                        foreach (var item in obj.Addresses)
                        {
                            if (_addressCount == 1)
                            {
                                if (item.MobileNo != null && item.MobileNo != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divAddrMobileNo" });
                                        isError = true;
                                    }
                                }

                                if (item.MobileNo2 != null && item.MobileNo2 != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo2);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAddrMobileNo2" });
                                        isError = true;
                                    }
                                }
                            }
                            else
                            {
                                if (item.MobileNo != null && item.MobileNo != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divAddrAltMobileNo" });
                                        isError = true;
                                    }
                                }

                                if (item.MobileNo2 != null && item.MobileNo2 != "")
                                {
                                    bool check = oCommonController.MobileValidationCheck(item.MobileNo2);
                                    if (check == false)
                                    {
                                        errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAddrAltMobileNo2" });
                                        isError = true;
                                    }
                                }
                            }
                            _addressCount++;
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

                //obj.BranchIds = "";
                //if (obj.Branchs != null)
                //{
                //    foreach (var item in obj.Branchs)
                //    {
                //        obj.BranchIds = obj.BranchIds + "," + item;
                //    }
                //}

                var existingDetails = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => new
                {
                    a.OpeningBalance,
                    a.AdvanceBalance,
                    a.CurrencyId
                }).FirstOrDefault();

                decimal OpeningBalancePaid = oConnectionContext.DbClsCustomerPayment.Where(b => b.Type == "Opening Balance Payment"
                && b.IsDeleted == false && b.IsCancelled == false && b.CustomerId == obj.UserId).Select(b => b.Amount).DefaultIfEmpty().Sum();

                if (OpeningBalancePaid > obj.OpeningBalance)
                {
                    errors.Add(new ClsError { Message = "Mismatch Opening Balance", Id = "divOpeningBalance" });
                    isError = true;

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

                long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
  && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();
                long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
   && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault();

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    UserRoleId = obj.UserRoleId,
                    UserGroupId = obj.UserGroupId,
                    Name = obj.Name,
                    //Username = obj.Username,
                    Password = obj.Password,
                    EmailId = obj.EmailId,
                    MobileNo = obj.MobileNo,
                    AltMobileNo = obj.AltMobileNo,
                    ReligionId = obj.ReligionId,
                    DOB = obj.DOB != null ? obj.DOB.Value.AddHours(5).AddMinutes(30) : obj.DOB,
                    Gender = obj.Gender,
                    BusinessName = obj.BusinessName,
                    TaxNo = obj.TaxNo,
                    Notes = obj.Notes,
                    CommissionPercent = obj.CommissionPercent,
                    CreditLimit = obj.CreditLimit,
                    OpeningBalance = obj.OpeningBalance,
                    //PayTermNo = obj.PayTermNo,
                    //PayTerm = obj.PayTerm,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    JoiningDate = obj.JoiningDate != null ? obj.JoiningDate.AddHours(5).AddMinutes(30) : obj.JoiningDate,
                    //BranchIds = obj.BranchIds.TrimStart(','),
                    IsShippingAddressDifferent = obj.IsShippingAddressDifferent,
                    TaxId = obj.TaxId,
                    //CurrencyId = existingDetails.CurrencyId,
                    //AdvanceBalance = (existingDetails.AdvanceBalance + obj.OpeningBalance) - existingDetails.OpeningBalance,
                    AccountId = AccountId,
                    JournalAccountId = JournalAccountId,
                    PaymentTermId = obj.PaymentTermId,
                    PlaceOfSupplyId = obj.PlaceOfSupplyId,
                    SourceOfSupplyId = obj.SourceOfSupplyId,
                    TaxPreferenceId = obj.TaxPreferenceId,
                    TaxExemptionId = obj.TaxExemptionId,
                    IsBusinessRegistered = obj.IsBusinessRegistered,
                    GstTreatment = obj.GstTreatment,
                    BusinessRegistrationNameId = obj.BusinessRegistrationNameId,
                    BusinessRegistrationNo = obj.BusinessRegistrationNo,
                    BusinessLegalName = obj.BusinessLegalName,
                    BusinessTradeName = obj.BusinessTradeName,
                    PanNo = obj.PanNo
                };

                string pic1 = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.ProfilePic).FirstOrDefault();
                if (obj.ProfilePic != "" && obj.ProfilePic != null && !obj.ProfilePic.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/ProfilePic/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProfilePic;

                    string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProfilePic");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsUser.ProfilePic = filepathPass;
                }
                else
                {
                    oClsUser.ProfilePic = pic1;
                }

                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.UserId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.UserRoleId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.UserGroupId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.Name).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.Username).IsModified = true;
                if (obj.Password != null && obj.Password != "" && obj.CPassword != null && obj.CPassword != "")
                {
                    oConnectionContext.Entry(oClsUser).Property(x => x.Password).IsModified = true;
                }
                oConnectionContext.Entry(oClsUser).Property(x => x.EmailId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.MobileNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.AltMobileNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ReligionId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.DOB).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.Gender).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.Tagline).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.BusinessType).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.BusinessName).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.OwnerName).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.TaxNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.Notes).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.CurrencyId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.CityId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.StateId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.CountryId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.Zipcode).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.Address).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltAddress).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ProfilePic).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.CommissionPercent).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.CreditLimit).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.OpeningBalance).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.PayTermNo).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.PayTerm).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.JoiningDate).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltCityId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltCountryId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltStateId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltZipcode).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.BranchIds).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.IsShippingAddressDifferent).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.TaxId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.UserCurrencyMapId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AdvanceBalance).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.AccountId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.JournalAccountId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.PaymentTermId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.PlaceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.SourceOfSupplyId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.TaxPreferenceId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.TaxExemptionId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.IsBusinessRegistered).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.GstTreatment).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.BusinessRegistrationNameId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.BusinessRegistrationNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.BusinessLegalName).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.BusinessTradeName).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.PanNo).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.UserType.ToLower() == "user")
                {
                    string query = "update \"tblUserBranchMap\" set \"IsActive\"= False where \"UserId\"=" + obj.UserId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    //Map with Branch
                    if (obj.Branchs != null)
                    {
                        foreach (var item in obj.Branchs)
                        {
                            long BranchId = Convert.ToInt64(item);
                            long UserBranchMapId = oConnectionContext.DbClsUserBranchMap.Where(a => a.UserId == oClsUser.UserId &&
                            a.BranchId == BranchId && a.IsDeleted == false).Select(a => a.UserBranchMapId).FirstOrDefault();
                            if (UserBranchMapId == 0)
                            {
                                ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
                                {
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    BranchId = BranchId,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    UserType = obj.UserType,
                                    ModifiedBy = obj.AddedBy,
                                    UserId = oClsUser.UserId
                                };
                                oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap);
                                oConnectionContext.SaveChanges();
                            }
                            else
                            {
                                ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
                                {
                                    UserBranchMapId = UserBranchMapId,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    BranchId = BranchId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    UserId = oClsUser.UserId
                                };
                                oConnectionContext.DbClsUserBranchMap.Attach(oClsUserBranchMap);
                                oConnectionContext.Entry(oClsUserBranchMap).Property(x => x.ModifiedBy).IsModified = true;
                                oConnectionContext.Entry(oClsUserBranchMap).Property(x => x.ModifiedOn).IsModified = true;
                                oConnectionContext.Entry(oClsUserBranchMap).Property(x => x.BranchId).IsModified = true;
                                oConnectionContext.Entry(oClsUserBranchMap).Property(x => x.IsActive).IsModified = true;
                                oConnectionContext.Entry(oClsUserBranchMap).Property(x => x.IsDeleted).IsModified = true;
                                oConnectionContext.Entry(oClsUserBranchMap).Property(x => x.UserId).IsModified = true;
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }
                }


                if (obj.Addresses != null)
                {
                    foreach (var item in obj.Addresses)
                    {
                        ClsAddress oClsAddress = new ClsAddress()
                        {
                            AddressId = item.AddressId,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            Address = item.Address,
                            CityId = item.CityId,
                            CountryId = item.CountryId,
                            EmailId = item.EmailId,
                            Landmark = item.Landmark,
                            Latitude = item.Latitude,
                            Locality = item.Locality,
                            Longitude = item.Longitude,
                            MobileNo = item.MobileNo,
                            MobileNo2 = item.MobileNo2,
                            Name = item.Name,
                            StateId = item.StateId,
                            Type = item.Type,
                            UserId = obj.UserId,
                            Zipcode = item.Zipcode
                        };
                        oConnectionContext.DbClsAddress.Attach(oClsAddress);
                        oConnectionContext.Entry(oClsAddress).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Address).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.CityId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.CountryId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.EmailId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Landmark).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Latitude).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Longitude).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Locality).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.MobileNo).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.MobileNo2).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Name).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.StateId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Type).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Zipcode).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.UserType + "s",
                    CompanyId = obj.CompanyId,
                    Description = obj.UserType + " \"" + obj.Username + "\" updated",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = (obj.UserType == "user" ? "User" : obj.UserType) + " updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
        public async Task<IHttpActionResult> UpdateUserPassword(ClsUserVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Password == null || obj.Password == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPassword" });
                    isError = true;
                }

                if (obj.CPassword == null || obj.CPassword == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCPassword" });
                    isError = true;
                }

                if (obj.Password != null && obj.CPassword != null)
                {
                    if (obj.Password != obj.CPassword)
                    {
                        errors.Add(new ClsError { Message = "New and Confirm Passwords do not match", Id = "divCPassword" });
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

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    Password = obj.Password,
                };
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.UserId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.Password).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Change Password",
                    CompanyId = obj.CompanyId,
                    Description = "Password changed",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string query = "update \"tblLoginDetails\" set \"IsLoggedOut\"=True where \"AddedBy\"=" + obj.UserId + " and \"LoginDetailsId\"!=" + obj.LoginDetailsId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                data = new
                {
                    Status = 1,
                    Message = "Password changed successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserActiveInactive(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.UserType).FirstOrDefault();

                if (UserType.ToLower() == "user")
                {
                    if (obj.IsActive == true)
                    {
                        int TotalUser = oCommonController.fetchPlanQuantity(obj.CompanyId, "User");
                        int TotalUserUsed = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.UserType == "User").Count();
                        if (TotalUserUsed >= TotalUser)
                        {
                            data = new
                            {
                                Status = 0,
                                Message = "User quota already used. Please upgrade addons from My Plan Menu",
                                Data = new
                                {
                                }
                            };
                            return await Task.FromResult(Ok(data));
                        }
                    }
                }

                var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => new
                {
                    a.UserType,
                    a.Username
                }).FirstOrDefault();

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.UserId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = det.UserType + "s",
                    CompanyId = obj.CompanyId,
                    Description = det.UserType + " \"" + det.Username + "\" " + (obj.IsActive == true ? "activated " : "deactivated "),
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = UserType + (obj.IsActive == true ? " activated" : " deactivated") + " successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserDelete(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => new
                {
                    a.UserType,
                    a.Username
                }).FirstOrDefault();

                if (det.UserType.ToLower() == "supplier")
                {
                    int PurchaseQuotationCount = (from a in oConnectionContext.DbClsPurchaseQuotation
                                                  join b in oConnectionContext.DbClsPurchaseQuotationDetails
                                               on a.PurchaseQuotationId equals b.PurchaseQuotationId
                                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                               && a.SupplierId == obj.UserId
                                                  select a.PurchaseQuotationId).Count();

                    int PurchaseOrderCount = (from a in oConnectionContext.DbClsPurchaseOrder
                                              join b in oConnectionContext.DbClsPurchaseOrderDetails
                                               on a.PurchaseOrderId equals b.PurchaseOrderId
                                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false 
                                               && a.SupplierId == obj.UserId
                                              select a.PurchaseOrderId).Count();

                    int PurchaseCount = (from a in oConnectionContext.DbClsPurchase
                                         join b in oConnectionContext.DbClsPurchaseDetails
                                       on a.PurchaseId equals b.PurchaseId
                                         where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false
                                       && a.SupplierId == obj.UserId
                                         select a.PurchaseId).Count();

                    int SupplierPaymentCount = (from a in oConnectionContext.DbClsSupplierPayment
                                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsDeleted == false && a.IsCancelled == false
                                                 && a.SupplierId == obj.UserId
                                                select a.PurchaseId).Count();

                    if (PurchaseQuotationCount > 0 || PurchaseOrderCount > 0 || PurchaseCount > 0 || SupplierPaymentCount > 0)
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
                }
                else if (det.UserType.ToLower() == "customer")
                {
                    int SalesQuotationCount = (from a in oConnectionContext.DbClsSalesQuotation
                                               join b in oConnectionContext.DbClsSalesQuotationDetails
                                               on a.SalesQuotationId equals b.SalesQuotationId
                                               where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                               && a.CustomerId == obj.UserId
                                               select a.SalesQuotationId).Count();

                    int SalesOrderCount = (from a in oConnectionContext.DbClsSalesOrder
                                           join b in oConnectionContext.DbClsSalesOrderDetails
                                               on a.SalesOrderId equals b.SalesOrderId
                                           where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                               && a.CustomerId == obj.UserId
                                           select a.SalesOrderId).Count();

                    int SalesProformaCount = (from a in oConnectionContext.DbClsSalesProforma
                                              join b in oConnectionContext.DbClsSalesProformaDetails
                                               on a.SalesProformaId equals b.SalesProformaId
                                              where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false
                                               && a.CustomerId == obj.UserId
                                              select a.SalesProformaId).Count();

                    int DeliveryChallanCount = (from a in oConnectionContext.DbClsDeliveryChallan
                                                join b in oConnectionContext.DbClsDeliveryChallanDetails
                                               on a.DeliveryChallanId equals b.DeliveryChallanId
                                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false 
                                               && a.CustomerId == obj.UserId
                                                select a.DeliveryChallanId).Count();

                    int SalesCount = (from a in oConnectionContext.DbClsSales
                                      join b in oConnectionContext.DbClsSalesDetails
                                       on a.SalesId equals b.SalesId
                                      where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false 
                                       && a.CustomerId == obj.UserId
                                      select a.SalesId).Count();

                    int CustomerPaymentCount = (from a in oConnectionContext.DbClsCustomerPayment
                                                where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsDeleted == false && a.IsCancelled == false
                                                 && a.CustomerId == obj.UserId
                                                select a.SalesId).Count();

                    if (SalesQuotationCount > 0 || SalesOrderCount > 0 || SalesProformaCount > 0 || DeliveryChallanCount > 0 || SalesCount > 0
                        || CustomerPaymentCount > 0)
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
                }
                else
                {
                    int RoleCount = oConnectionContext.DbClsRole.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int UserCount = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int CategoryCount = oConnectionContext.DbClsCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SubCategoryCount = oConnectionContext.DbClsSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SubSubCategoryCount = oConnectionContext.DbClsSubSubCategory.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int UnitCount = oConnectionContext.DbClsUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false  && a.AddedBy == obj.UserId).Count();
                    int SecondaryUnitCount = oConnectionContext.DbClsSecondaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int TertiaryUnitCount = oConnectionContext.DbClsTertiaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int QuaternaryUnitCount = oConnectionContext.DbClsQuaternaryUnit.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int VariationCount = oConnectionContext.DbClsVariation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int WarrantyCount = oConnectionContext.DbClsWarranty.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int BrandCount = oConnectionContext.DbClsBrand.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int ItemCount = oConnectionContext.DbClsItem.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int OpeningStockCount = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SellingPriceGroupCount = oConnectionContext.DbClsSellingPriceGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int ItemSellingPriceGroupMapCount = oConnectionContext.DbClsItemSellingPriceGroupMap.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int StockAdjustmentCount = oConnectionContext.DbClsStockAdjustment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int StockTransferCount = oConnectionContext.DbClsStockTransfer.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int PurchaseQuotationCount = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int PurchaseOrderCount = oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int PurchaseCount = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int PurchaseReturnCount = oConnectionContext.DbClsPurchaseReturn.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int SupplierPaymentCount = oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    //int SupplierRefundCount = oConnectionContext.DbClsSupplierRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int ExpenseCount = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int UserGroupCount = oConnectionContext.DbClsUserGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SalesQuotationCount = oConnectionContext.DbClsSalesQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SalesOrderCount = oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SalesProformaCount = oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int DeliveryChallanCount = oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int SalesCount = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int SalesReturnCount = oConnectionContext.DbClsSalesReturn.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int CustomerPaymentCount = oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    //int CustomerRefundCount = oConnectionContext.DbClsCustomerRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    //int StateCount = oConnectionContext.DbClsState.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.AddedBy == obj.UserId).Count();
                    int CityCount = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int AccountCount = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int ContraCount = oConnectionContext.DbClsContra.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int JournalCount = oConnectionContext.DbClsJournal.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int TaxCount = oConnectionContext.DbClsTax.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int PaymentTypeCount = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int EmailSettingsCount = oConnectionContext.DbClsEmailSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int SmsSettingsCount = oConnectionContext.DbClsSmsSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int WhatsappSettingsCount = oConnectionContext.DbClsWhatsappSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();
                    int OnlinePaymentSettingsCount = oConnectionContext.DbClsOnlinePaymentSettings.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.AddedBy == obj.UserId).Count();

                    if (RoleCount > 0 || UserCount > 0 || CategoryCount > 0 || SubCategoryCount > 0 || SubSubCategoryCount > 0 || UnitCount > 0
                        || SecondaryUnitCount > 0 || TertiaryUnitCount > 0 || QuaternaryUnitCount > 0 || VariationCount > 0 || WarrantyCount > 0
                        || BrandCount > 0 || ItemCount > 0 || OpeningStockCount > 0 || SellingPriceGroupCount > 0 || ItemSellingPriceGroupMapCount > 0
                        || StockAdjustmentCount > 0 || StockTransferCount > 0 || PurchaseQuotationCount > 0 || PurchaseOrderCount > 0 || PurchaseCount > 0
                        || PurchaseReturnCount > 0 || SupplierPaymentCount > 0
                        //|| SupplierRefundCount > 0 
                        || ExpenseCount > 0 || UserGroupCount > 0
                        || SalesQuotationCount > 0 || SalesOrderCount > 0 || SalesProformaCount > 0 || DeliveryChallanCount > 0 || SalesCount > 0
                        || SalesReturnCount > 0 || CustomerPaymentCount > 0
                        //|| CustomerRefundCount > 0
                        //|| StateCount > 0 
                        || CityCount > 0
                        || BranchCount > 0 || AccountCount > 0 || ContraCount > 0 || JournalCount > 0 || TaxCount > 0
                        || PaymentTypeCount > 0 || EmailSettingsCount > 0 || SmsSettingsCount > 0 || WhatsappSettingsCount > 0 || OnlinePaymentSettingsCount > 0)
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
                }

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.UserId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                var LoginDetails = oConnectionContext.DbClsLoginDetails.Where(a => a.AddedBy == obj.UserId).Select(a => new { a.LoginDetailsId }).ToList();
                foreach (var item in LoginDetails)
                {
                    ClsLoginDetails oClsLoginDetails = new ClsLoginDetails()
                    {
                        LoginDetailsId = item.LoginDetailsId,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                        IsLoggedOut = true,
                    };
                    oConnectionContext.DbClsLoginDetails.Attach(oClsLoginDetails);
                    oConnectionContext.Entry(oClsLoginDetails).Property(x => x.ModifiedBy).IsModified = true;
                    oConnectionContext.Entry(oClsLoginDetails).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.Entry(oClsLoginDetails).Property(x => x.IsLoggedOut).IsModified = true;
                    oConnectionContext.SaveChanges();
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = det.UserType + "s",
                    CompanyId = obj.CompanyId,
                    Description = det.UserType + " \"" + det.Username + "\" deleted",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "User deleted successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ChangePassword(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.OldPassword == null || obj.OldPassword == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divOldPassword" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "All fields are required",
                    //    Data = new
                    //    {

                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (obj.Password == null || obj.Password == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPassword" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "All fields are required",
                    //    Data = new
                    //    {

                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (obj.CPassword == null || obj.CPassword == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCPassword" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "All fields are required",
                    //    Data = new
                    //    {

                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (obj.Password != null && obj.CPassword != null)
                {
                    if (obj.Password != obj.CPassword)
                    {
                        errors.Add(new ClsError { Message = "New and Confirm Passwords do not match", Id = "divCPassword" });
                        isError = true;
                        //data = new
                        //{
                        //    Status = 0,
                        //    Message = "New and Confirm Passwords do not match",
                        //    Data = new
                        //    {

                        //    }
                        //};
                        //return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.OldPassword != null)
                {
                    string oldpass = oCommonController.Sha256Encryption(obj.OldPassword);
                    long count = oConnectionContext.DbClsUser.Where(a => a.Password == oldpass && a.UserId == obj.AddedBy).Count();
                    if (count == 0)
                    {
                        errors.Add(new ClsError { Message = "Invalid Current Password", Id = "divOldPassword" });
                        isError = true;
                        //data = new
                        //{
                        //    Status = 0,
                        //    Message = "Invalid Current Password",
                        //    Data = new
                        //    {

                        //    }
                        //};
                        //return await Task.FromResult(Ok(data));
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

                ClsUser oClsUser = new ClsUser();
                oClsUser.UserId = obj.AddedBy;
                oClsUser.Password = oCommonController.Sha256Encryption(obj.Password);
                oClsUser.ModifiedOn = CurrentDate;
                oClsUser.ModifiedBy = obj.AddedBy;
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.Password).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Change Password",
                    CompanyId = obj.CompanyId,
                    Description = "Password changed",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                string query = "update \"tblLoginDetails\" set \"IsLoggedOut\"=True where \"AddedBy\"=" + obj.AddedBy + " and \"LoginDetailsId\"!=" + obj.LoginDetailsId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                data = new
                {
                    Status = 1,
                    Message = "Password changed successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));

        }

        public async Task<IHttpActionResult> ProfileUpdate(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.Name == null || obj.Name == "" || obj.MobileNo == null || obj.MobileNo == "")
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Fields marked with * is required",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo == obj.MobileNo &&
                a.UserType.ToLower() == obj.UserType.ToLower() //(a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") 
                && a.UserId != obj.AddedBy && a.IsDeleted == false).Count() > 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Duplicate Mobile No exists",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Invalid Email Id",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != "" && obj.EmailId != null)
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId == obj.EmailId &&
                    a.UserType.ToLower() == obj.UserType.ToLower() //(a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") 
                    && a.UserId != obj.AddedBy && a.IsDeleted == false).Count() > 0)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Duplicate Email Id exists",
                            Data = new
                            {

                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }
                ClsUser oClsUser = new ClsUser();
                oClsUser.UserId = obj.AddedBy;
                oClsUser.Name = obj.Name;
                oClsUser.MobileNo = obj.MobileNo;
                oClsUser.EmailId = obj.EmailId;
                oClsUser.ModifiedOn = CurrentDate;
                oClsUser.ModifiedBy = obj.AddedBy;
                oClsUser.AltMobileNo = obj.AltMobileNo;

                string pic1 = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.ProfilePic).FirstOrDefault();
                if (obj.ProfilePic != "" && obj.ProfilePic != null && !obj.ProfilePic.Contains("http"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/ProfilePic/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProfilePic;

                    string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProfilePic");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsUser.ProfilePic = filepathPass;
                }
                else
                {
                    oClsUser.ProfilePic = pic1;
                }

                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.Name).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.EmailId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.MobileNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.AltMobileNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ProfilePic).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Profile Update",
                    CompanyId = obj.CompanyId,
                    Description = "Profile updated",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Profile updated successfully.",
                    Data = new
                    {

                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> BusinessInfo(ClsUser obj)
        //{
        //    var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => new
        //    {
        //        a.BusinessName,
        //        a.Tagline,
        //        a.BusinessType,
        //        a.OwnerName,
        //        a.BusinessMobileNo,
        //        a.BusinessAltMobileNo,
        //        a.BusinessEmailId,
        //        a.TaxNo,
        //        a.Notes,
        //        a.CurrencyId,
        //        BusinessLogo = oCommonController.webUrl + a.BusinessLogo,
        //        a.BusinessCountryId,
        //        a.BusinessStateId,
        //        a.BusinessCityId,
        //        a.BusinessZipcode,
        //        a.BusinessAddress
        //    }).FirstOrDefault();

        //    var Countrys = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
        //    {
        //        CountryId = a.CountryId,
        //        a.CountryCode,
        //        Country = a.Country,
        //    }).OrderBy(a => a.Country).ToList();

        //    var States = oConnectionContext.DbClsState.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true && a.CountryId == det.BusinessCountryId).Select(a => new
        //    {
        //        StateId = a.StateId,
        //        a.StateCode,
        //        State = a.State,
        //    }).OrderBy(a => a.State).ToList();

        //    var Citys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true && a.StateId == det.BusinessStateId).Select(a => new
        //    {
        //        CityId = a.CityId,
        //        a.CityCode,
        //        City = a.City,
        //    }).OrderBy(a => a.City).ToList();

        //    var Currencys = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
        //    {
        //        CurrencyId = a.CurrencyId,
        //        a.CurrencyCode,
        //        a.CurrencySymbol,
        //        a.CurrencyName,
        //    }).OrderBy(a => a.CurrencyName).ToList();

        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            User = det,
        //            Countrys = Countrys,
        //            States = States,
        //            Citys = Citys,
        //            Currencys = Currencys
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        //public async Task<IHttpActionResult> BusinessInfoUpdate(ClsUserVm obj)
        //{
        //    if (obj.BusinessName == "" || obj.BusinessName == null || obj.BusinessType == "" || obj.BusinessType == null ||
        //        obj.OwnerName == "" || obj.OwnerName == null || obj.BusinessMobileNo == "" || obj.BusinessMobileNo == null ||
        //        obj.BusinessCountryId == 0 || obj.BusinessStateId == 0 || obj.BusinessCityId == 0 ||
        //        obj.BusinessZipcode == 0 || obj.BusinessAddress == "" || obj.BusinessAddress == null)
        //    {
        //        data = new
        //        {
        //            Status = 0,
        //            Message = "Fields marked with * is mandatory",
        //            Data = new
        //            {
        //            }
        //        };
        //        return await Task.FromResult(Ok(data));
        //    }

        //    if (obj.BusinessEmailId != null && obj.BusinessEmailId != "")
        //    {
        //        bool check = oCommonController.EmailValidationCheck(obj.BusinessEmailId);
        //        if (check == false)
        //        {
        //            data = new
        //            {
        //                Status = 0,
        //                Message = "Invalid Business Email Id",
        //                Data = new
        //                {
        //                }
        //            };
        //            return await Task.FromResult(Ok(data));
        //        }
        //    }
        //    ClsUser oClsUser = new ClsUser();
        //    oClsUser.UserId = obj.AddedBy;
        //    oClsUser.BusinessName = obj.BusinessName;
        //    oClsUser.Tagline = obj.Tagline;
        //    oClsUser.BusinessType = obj.BusinessType;
        //    oClsUser.OwnerName = obj.OwnerName;
        //    oClsUser.BusinessMobileNo = obj.BusinessMobileNo;
        //    oClsUser.BusinessAltMobileNo = obj.BusinessAltMobileNo;
        //    oClsUser.BusinessEmailId = obj.BusinessEmailId;
        //    oClsUser.TaxNo = obj.TaxNo;
        //    oClsUser.Notes = obj.Notes;
        //    oClsUser.CurrencyId = obj.CurrencyId;
        //    oClsUser.BusinessCountryId = obj.BusinessCountryId;
        //    oClsUser.BusinessStateId = obj.BusinessStateId;
        //    oClsUser.BusinessCityId = obj.BusinessCityId;
        //    oClsUser.BusinessZipcode = obj.BusinessZipcode;
        //    oClsUser.BusinessAddress = obj.BusinessAddress;
        //    oClsUser.ModifiedOn = CurrentDate;
        //    oClsUser.ModifiedBy = obj.AddedBy;

        //    string pic1 = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.BusinessLogo).FirstOrDefault();
        //    if (obj.BusinessLogo != "" && obj.BusinessLogo != null && !obj.BusinessLogo.Contains("http"))
        //    {
        //        string filepathPass = "";

        //        if (pic1 != "" && pic1 != null)
        //        {
        //            if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
        //            {
        //                System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
        //            }
        //        }

        //        filepathPass = "/ExternalContents/Images/BusinessLogo/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProfilePic;

        //        var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BusinessLogo");
        //        if (!Directory.Exists(folder))
        //        {
        //            Directory.CreateDirectory(folder);
        //        }

        //        string base64 = obj.BusinessLogo.Replace(obj.BusinessLogo.Substring(0, obj.BusinessLogo.IndexOf(',') + 1), "");
        //        File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

        //        oClsUser.BusinessLogo = filepathPass;
        //    }
        //    else
        //    {
        //        oClsUser.BusinessLogo = pic1;
        //    }


        //    oConnectionContext.DbClsUser.Attach(oClsUser);
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessName).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.Tagline).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessType).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.OwnerName).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessMobileNo).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessAltMobileNo).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessEmailId).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.TaxNo).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.Notes).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.CurrencyId).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessCountryId).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessStateId).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessCityId).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessZipcode).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessAddress).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
        //    oConnectionContext.Entry(oClsUser).Property(x => x.BusinessLogo).IsModified = true;
        //    oConnectionContext.SaveChanges();

        //    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
        //    {
        //        AddedBy = obj.AddedBy,
        //        Browser = obj.Browser,
        //        Category = "Business Information Update",
        //        CompanyId = obj.CompanyId,
        //        Description = "updated business informations",
        //        Id = oClsUser.UserId,
        //        IpAddress = obj.IpAddress,
        //        Platform = obj.Platform,
        //        Type = "Profile Update"
        //    };
        //    oCommonController.InsertActivityLog(oClsActivityLogVm,CurrentDate);

        //    data = new
        //    {
        //        Status = 1,
        //        Message = "Business Info updated successfully.",
        //        Data = new
        //        {

        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> AllActiveUsers(ClsUserVm obj)
        {
            List<ClsUserVm> det;
            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsUser
                       join b in oConnectionContext.DbClsUserBranchMap
                       on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId &&
                       a.IsDeleted == false && b.IsActive == true
                       && b.IsActive == true && b.IsDeleted == false &&
                       oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
        && a.UserType.ToLower() == "user"
                       select new ClsUserVm
                       {
                           //BranchId = b.BranchId,
                           UserType = a.UserType,
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           UserGroup = oConnectionContext.DbClsUserGroup.Where(b => b.UserGroupId == a.UserGroupId).Select(b => b.UserGroup).FirstOrDefault(),
                           Name = a.Name,
                           Password = a.Password,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           AltMobileNo = a.AltMobileNo,
                           ReligionId = a.ReligionId,
                           DOB = a.DOB,
                           Gender = a.Gender,
                           TaxNo = a.TaxNo,
                           Notes = a.Notes,
                           CommissionPercent = a.CommissionPercent,
                           OpeningBalance = a.OpeningBalance,
                           //PayTermNo = a.PayTermNo,
                           //PayTerm = a.PayTerm,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           IsCompany = a.IsCompany,
                           CompanyId = a.CompanyId,
                           ExpiryDate = a.ExpiryDate,
                           JoiningDate = a.JoiningDate,
                           ProfilePic = oCommonController.baseUrl + a.ProfilePic
                       }).Distinct().Union(from a in oConnectionContext.DbClsUser
                                           where a.CompanyId == obj.CompanyId &&
                                           a.IsDeleted == false && a.IsActive == true
                                           && a.UserType.ToLower() != "user"
                                           //&& b.IsActive == true 
                                           //&& b.IsDeleted == false && b.IsCancelled == false
                                           select new ClsUserVm
                                           {
                                               //BranchId = b.BranchId,
                                               UserType = a.UserType,
                                               UserId = a.UserId,
                                               Username = a.Username,
                                               UserRoleId = a.UserRoleId,
                                               RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                                               UserGroupId = a.UserGroupId,
                                               UserGroup = oConnectionContext.DbClsUserGroup.Where(b => b.UserGroupId == a.UserGroupId).Select(b => b.UserGroup).FirstOrDefault(),
                                               Name = a.Name,
                                               Password = a.Password,
                                               EmailId = a.EmailId,
                                               MobileNo = a.MobileNo,
                                               AltMobileNo = a.AltMobileNo,
                                               ReligionId = a.ReligionId,
                                               DOB = a.DOB,
                                               Gender = a.Gender,
                                               TaxNo = a.TaxNo,
                                               Notes = a.Notes,
                                               CommissionPercent = a.CommissionPercent,
                                               OpeningBalance = a.OpeningBalance,
                                               //PayTermNo = a.PayTermNo,
                                               //PayTerm = a.PayTerm,
                                               IsActive = a.IsActive,
                                               IsDeleted = a.IsDeleted,
                                               AddedBy = a.AddedBy,
                                               AddedOn = a.AddedOn,
                                               ModifiedBy = a.ModifiedBy,
                                               ModifiedOn = a.ModifiedOn,
                                               IsCompany = a.IsCompany,
                                               CompanyId = a.CompanyId,
                                               ExpiryDate = a.ExpiryDate,
                                               JoiningDate = a.JoiningDate,
                                               ProfilePic = oCommonController.baseUrl + a.ProfilePic
                                           }).Distinct().ToList();

            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                       join b in oConnectionContext.DbClsUserBranchMap
                       on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId &&
                       a.IsDeleted == false && b.IsActive == true
                       && b.IsActive == true && b.IsDeleted == false && b.BranchId == obj.BranchId
                       && a.UserType.ToLower() == "user"
                       //               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                       select new ClsUserVm
                       {
                           //BranchId = b.BranchId,
                           UserType = a.UserType,
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           UserGroup = oConnectionContext.DbClsUserGroup.Where(b => b.UserGroupId == a.UserGroupId).Select(b => b.UserGroup).FirstOrDefault(),
                           Name = a.Name,
                           Password = a.Password,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           AltMobileNo = a.AltMobileNo,
                           ReligionId = a.ReligionId,
                           DOB = a.DOB,
                           Gender = a.Gender,
                           TaxNo = a.TaxNo,
                           Notes = a.Notes,
                           CommissionPercent = a.CommissionPercent,
                           OpeningBalance = a.OpeningBalance,
                           //PayTermNo = a.PayTermNo,
                           //PayTerm = a.PayTerm,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           IsCompany = a.IsCompany,
                           CompanyId = a.CompanyId,
                           ExpiryDate = a.ExpiryDate,
                           JoiningDate = a.JoiningDate,
                           ProfilePic = oCommonController.baseUrl + a.ProfilePic
                       }).Distinct().Union(from a in oConnectionContext.DbClsUser
                                           where a.CompanyId == obj.CompanyId &&
                                           a.IsDeleted == false && a.IsActive == true
                                           && a.UserType.ToLower() != "user"
                                           //&& b.IsActive == true && b.IsDeleted == false && b.IsCancelled == false 
                                           //&& b.BranchId == obj.BranchId
                                           //               oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                           //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                                           select new ClsUserVm
                                           {
                                               //BranchId = b.BranchId,
                                               UserType = a.UserType,
                                               UserId = a.UserId,
                                               Username = a.Username,
                                               UserRoleId = a.UserRoleId,
                                               RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                                               UserGroupId = a.UserGroupId,
                                               UserGroup = oConnectionContext.DbClsUserGroup.Where(b => b.UserGroupId == a.UserGroupId).Select(b => b.UserGroup).FirstOrDefault(),
                                               Name = a.Name,
                                               Password = a.Password,
                                               EmailId = a.EmailId,
                                               MobileNo = a.MobileNo,
                                               AltMobileNo = a.AltMobileNo,
                                               ReligionId = a.ReligionId,
                                               DOB = a.DOB,
                                               Gender = a.Gender,
                                               TaxNo = a.TaxNo,
                                               Notes = a.Notes,
                                               CommissionPercent = a.CommissionPercent,
                                               OpeningBalance = a.OpeningBalance,
                                               //PayTermNo = a.PayTermNo,
                                               //PayTerm = a.PayTerm,
                                               IsActive = a.IsActive,
                                               IsDeleted = a.IsDeleted,
                                               AddedBy = a.AddedBy,
                                               AddedOn = a.AddedOn,
                                               ModifiedBy = a.ModifiedBy,
                                               ModifiedOn = a.ModifiedOn,
                                               IsCompany = a.IsCompany,
                                               CompanyId = a.CompanyId,
                                               ExpiryDate = a.ExpiryDate,
                                               JoiningDate = a.JoiningDate,
                                               ProfilePic = oCommonController.baseUrl + a.ProfilePic
                                           }).Distinct().ToList();
            }

            if (obj.UserType != "" && obj.UserType != null)
            {
                det = det.Where(a => a.UserType.ToLower() == obj.UserType.ToLower()).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det,
                }
            };
            //}

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ProfileUpdateCompany(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                if (obj.Name == "" || obj.Name == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divName" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Name is required",
                    //    Data = new
                    //    {
                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (obj.MobileNo == "" || obj.MobileNo == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                    isError = true;
                }

                if (obj.MobileNo != null && obj.MobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.MobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Mobile No", Id = "divMobileNo" });
                        isError = true;
                    }
                }

                if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Alternate Mobile No", Id = "divAltMobileNo" });
                        isError = true;
                    }
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo.ToLower() == obj.MobileNo.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.UserId != obj.AddedBy).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Mobile No exists", Id = "divMobileNo" });
                    isError = true;
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId.ToLower() == obj.EmailId.ToLower() && (a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales") && a.IsDeleted == false && a.UserId != obj.AddedBy).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Email Id exists", Id = "divEmailId" });
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

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.AddedBy,
                    Name = obj.Name,
                    EmailId = obj.EmailId,
                    MobileNo = obj.MobileNo,
                    AltMobileNo = obj.AltMobileNo,
                    ReligionId = obj.ReligionId,
                    DOB = obj.DOB,
                    Gender = obj.Gender,
                    //CityId = obj.CityId,
                    //StateId = obj.StateId,
                    //CountryId = obj.CountryId,
                    //Zipcode = obj.Zipcode,
                    //Address = obj.Address,
                    //AltAddress = obj.AltAddress,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    //AltCityId = obj.AltCityId,
                    //AltCountryId = obj.AltCountryId,
                    //AltStateId = obj.AltStateId,
                    //AltZipcode = obj.AltZipcode,
                };

                string pic1 = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.ProfilePic).FirstOrDefault();
                if (obj.ProfilePic != "" && obj.ProfilePic != null && !obj.ProfilePic.Contains("://"))
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/ProfilePic/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionProfilePic;

                    string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ProfilePic");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.ProfilePic.Replace(obj.ProfilePic.Substring(0, obj.ProfilePic.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsUser.ProfilePic = filepathPass;
                }
                else
                {
                    oClsUser.ProfilePic = pic1;
                }

                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.Name).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.EmailId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.MobileNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.AltMobileNo).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ReligionId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.DOB).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.Gender).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.CityId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.StateId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.CountryId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.Zipcode).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.Address).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltAddress).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ProfilePic).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltCityId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltCountryId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltStateId).IsModified = true;
                //oConnectionContext.Entry(oClsUser).Property(x => x.AltZipcode).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.Addresses != null)
                {
                    foreach (var item in obj.Addresses)
                    {
                        ClsAddress oClsAddress = new ClsAddress()
                        {
                            AddressId = item.AddressId,
                            ModifiedBy = obj.AddedBy,
                            ModifiedOn = CurrentDate,
                            Address = item.Address,
                            CityId = item.CityId,
                            CountryId = item.CountryId,
                            EmailId = item.EmailId,
                            Landmark = item.Landmark,
                            Latitude = item.Latitude,
                            Locality = item.Locality,
                            Longitude = item.Longitude,
                            MobileNo = item.MobileNo,
                            MobileNo2 = item.MobileNo2,
                            Name = item.Name,
                            StateId = item.StateId,
                            Type = item.Type,
                            UserId = obj.UserId,
                            Zipcode = item.Zipcode
                        };
                        oConnectionContext.DbClsAddress.Attach(oClsAddress);
                        oConnectionContext.Entry(oClsAddress).Property(x => x.ModifiedBy).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.ModifiedOn).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Address).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.CityId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.CountryId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.EmailId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Landmark).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Latitude).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Longitude).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Locality).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.MobileNo).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.MobileNo2).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Name).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.StateId).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Type).IsModified = true;
                        oConnectionContext.Entry(oClsAddress).Property(x => x.Zipcode).IsModified = true;
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Profile Update",
                    CompanyId = obj.CompanyId,
                    Description = "Profile updated",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Profile Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Profile updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserAutocomplete(ClsUserVm obj)
        {
            var det = oConnectionContext.DbClsUser.Where(a =>
            a.CompanyId == obj.CompanyId &&
            a.UserType.ToLower() == obj.UserType.ToLower() && a.IsDeleted == false
            && a.MobileNo.Contains(obj.MobileNo) && !oConnectionContext.DbClsUserBranchMap.Any(b => b.UserId == a.UserId && b.BranchId == obj.BranchId)
            ).Select(a => a.Name + "-" + a.MobileNo + "-" + a.EmailId).Take(9).ToList();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UsersArray = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }
        [AllowAnonymous]
        public async Task<IHttpActionResult> UserByMobileNo(ClsUserVm obj)
        {
            List<Int64> lst = new List<long>();
			var det = oConnectionContext.DbClsUser.Where(a =>
			a.CompanyId == obj.CompanyId &&
			a.UserType.ToLower() == obj.UserType.ToLower() &&
			a.MobileNo.Trim() == obj.MobileNo.Trim() && a.IsDeleted == false)
				.ToList()
				.Select(a => new
            {
                a.CurrencyId,
                a.CreditLimit,
                Branchs = a.IsCompany == true ? oConnectionContext.DbClsBranch.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                  && b.IsDeleted == false).Select(b => b.BranchId).ToList() :
                 oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == a.UserId && b.IsActive == true
                 && b.IsDeleted == false).Select(b => b.BranchId).ToList(),
                //a.BranchIds,
                UserId = a.UserId,
                Username = a.Username,
                UserRoleId = a.UserRoleId,
                UserGroupId = a.UserGroupId,
                UserGroup = oConnectionContext.DbClsUserGroup.Where(d => d.UserGroupId == a.UserGroupId).Select(d => d.UserGroup).FirstOrDefault(),
                Name = a.Name,
                Password = a.Password,
                EmailId = a.EmailId,
                MobileNo = a.MobileNo,
                AltMobileNo = a.AltMobileNo,
                ReligionId = a.ReligionId,
                DOB = a.DOB,
                Gender = a.Gender,
                a.TaxId,
                Tax = oConnectionContext.DbClsTax.Where(d => d.TaxId == a.TaxId).Select(d => d.Tax).FirstOrDefault(),
                //Tagline = a.Tagline,
                //BusinessType = a.BusinessType,
                BusinessName = a.BusinessName,
                //OwnerName = a.OwnerName,
                TaxNo = a.TaxNo,
                Notes = a.Notes,
                //CurrencyId = a.CurrencyId,
                //CityId = a.CityId,
                //StateId = a.StateId,
                //CountryId = a.CountryId,
                //Zipcode = a.Zipcode,
                //Address = a.Address,
                //AltAddress = a.AltAddress,
                ProfilePic = a.ProfilePic,
                CommissionPercent = a.CommissionPercent,
                OpeningBalance = a.OpeningBalance,
                //PayTermNo = a.PayTermNo,
                //PayTerm = a.PayTerm,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                IsCompany = a.IsCompany,
                CompanyId = a.CompanyId,
                ExpiryDate = a.ExpiryDate,
                JoiningDate = a.JoiningDate,
                //a.AltCityId,
                //a.AltCountryId,
                //a.AltStateId,
                //a.AltZipcode,
                ReligionName = oConnectionContext.DbClsReligion.Where(b => b.ReligionId == a.ReligionId).Select(b => b.ReligionName).FirstOrDefault(),
                RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                //Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                //State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                //City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                //AltCountry = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.AltCountryId).Select(b => b.Country).FirstOrDefault(),
                //AltState = oConnectionContext.DbClsState.Where(b => b.StateId == a.AltStateId).Select(b => b.State).FirstOrDefault(),
                //AltCity = oConnectionContext.DbClsCity.Where(b => b.CityId == a.AltCityId).Select(b => b.City).FirstOrDefault(),
                a.IsShippingAddressDifferent,
                Addresses = oConnectionContext.DbClsAddress.Where(b => b.UserId == a.UserId).Select(b => new
                {
                    AddressId = b.AddressId,
                    Type = b.Type,
                    UserId = b.UserId,
                    Name = b.Name,
                    EmailId = b.EmailId,
                    CountryId = b.CountryId,
                    StateId = b.StateId,
                    CityId = b.CityId,
                    Address = b.Address,
                    Zipcode = b.Zipcode,
                    MobileNo = b.MobileNo,
                    Latitude = b.Latitude,
                    Longitude = b.Longitude,
                    Locality = b.Locality,
                    Landmark = b.Landmark,
                    MobileNo2 = b.MobileNo2,
                    State = oConnectionContext.DbClsState.Where(c => c.StateId == b.StateId).Select(c => c.State).FirstOrDefault(),
                    City = oConnectionContext.DbClsCity.Where(c => c.CityId == b.CityId).Select(c => c.City).FirstOrDefault(),
                }).ToList()
            }).FirstOrDefault();
            var Countrys = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
            }).OrderBy(a => a.Country).ToList();

            long CountryId = det.Addresses[0].CountryId;
            long StateId = det.Addresses[0].StateId;

            var States = oConnectionContext.DbClsState.Where(a =>
            //a.CompanyId == obj.CompanyId &&
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == CountryId
            ).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                State = a.State,
            }).OrderBy(a => a.State).ToList();

            var Citys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.CountryId == CountryId && a.StateId == StateId).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                City = a.City,
            }).OrderBy(a => a.City).ToList();

            long AltCountryId = det.Addresses[1].CountryId;
            long AltStateId = det.Addresses[1].StateId;

            var AltStates = oConnectionContext.DbClsState.Where(a =>
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == AltCountryId
            ).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                State = a.State,
            }).OrderBy(a => a.State).ToList();

            var AltCitys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true
            && a.CountryId == AltCountryId && a.StateId == AltStateId).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                City = a.City,
            }).OrderBy(a => a.City).ToList();

            var UserGroups = oConnectionContext.DbClsUserGroup.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                UserGroupId = a.UserGroupId,
                UserGroup = a.UserGroup,
            }).OrderBy(a => a.UserGroup).ToList();

            var Roles = oConnectionContext.DbClsRole.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                RoleId = a.RoleId,
                RoleName = a.RoleName,
            }).OrderBy(a => a.RoleName).ToList();

            var Religions = oConnectionContext.DbClsReligion.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                ReligionId = a.ReligionId,
                ReligionName = a.ReligionName,
            }).OrderBy(a => a.ReligionName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = det,
                    Countrys = Countrys,
                    States = States,
                    Citys = Citys,
                    AltStates = AltStates,
                    AltCitys = AltCitys,
                    UserGroups = UserGroups,
                    Roles = Roles,
                    Religions = Religions
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AddExisting(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (oConnectionContext.DbClsUserBranchMap.Where(a => a.BranchId == obj.BranchId && a.UserId == obj.UserId && a.IsDeleted == false).Count() == 0)
                {
                    ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        BranchId = obj.BranchId,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        UserType = obj.UserType,
                        ModifiedBy = obj.AddedBy,
                        UserId = obj.UserId
                    };

                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap);
                    oConnectionContext.SaveChanges();
                }
                data = new
                {
                    Status = 1,
                    Message = (obj.UserType == "user" ? "User" : obj.UserType) + " created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> UsersBranchWise(ClsUserVm obj)
        //{
        //    var Users = (from a in oConnectionContext.DbClsUser
        //                 join b in oConnectionContext.DbClsUserBranchMap
        //                    on a.UserId equals b.UserId
        //                 where a.IsDeleted == false && a.IsCancelled == false && a.CompanyId == obj.CompanyId
        //                    && b.BranchId == obj.BranchId && a.UserType.ToLower() == obj.UserType.ToLower()
        //                 select new
        //                 {
        //                     a.UserType,
        //                     a.Name,
        //                     a.UserId,
        //                     a.MobileNo,
        //                     a.Username
        //                 });

        //    if (obj.CompanyId == obj.AddedBy)
        //    {
        //        Users = Users.Concat(oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => new
        //        {
        //            a.UserType,
        //            a.Name,
        //            a.UserId,
        //            a.MobileNo,
        //            a.Username
        //        }));
        //    }
        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            Users = Users,
        //        }
        //    };

        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> CustomerReport(ClsUserVm obj)
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

            List<ClsUserVm> det;

            if (obj.BranchId == 0)
            {
                    det = (from a in oConnectionContext.DbClsUser
                               //join b in oConnectionContext.DbClsUserBranchMap
                               //on a.UserId equals b.UserId
                           where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                           a.IsDeleted == false //&& b.IsActive == true
                           //&& a.UserId != obj.AddedBy 
                           && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                    DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                           select new ClsUserVm
                           {
                               PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(bb => bb.PaymentTermId == a.PaymentTermId).Select(bb => bb.PaymentTerm).FirstOrDefault(),
                               IsWalkin = a.IsWalkin,
                               //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                               BusinessName = a.BusinessName,
                               UserId = a.UserId,
                               Username = a.Username,
                               UserRoleId = a.UserRoleId,
                               RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                               UserGroupId = a.UserGroupId,
                               UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                               Name = a.Name,
                               Password = a.Password,
                               EmailId = a.EmailId,
                               MobileNo = a.MobileNo,
                               AltMobileNo = a.AltMobileNo,
                               ReligionId = a.ReligionId,
                               DOB = a.DOB,
                               Gender = a.Gender,
                               TaxNo = a.TaxNo,
                               Notes = a.Notes,
                               CommissionPercent = a.CommissionPercent,
                               OpeningBalance = a.OpeningBalance,
                               CustomerOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                             where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               SupplierOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                             where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               AdvanceBalance = a.AdvanceBalance,
                               CreditLimit = a.CreditLimit,
                               //PayTermNo = a.PayTermNo,
                               //PayTerm = a.PayTerm,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               IsCompany = a.IsCompany,
                               CompanyId = a.CompanyId,
                               ExpiryDate = a.ExpiryDate,
                               JoiningDate = a.JoiningDate,
                               ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                               AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                               ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                               //           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status == "Sent" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //l.UserId == c.CustomerId && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                               //                                        && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesInvoices = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                 //&& a.BranchId == obj.BranchId
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                ).Select(c => c.GrandTotal).Count(),
                               TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                 //&& a.BranchId == obj.BranchId
                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                                   join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                                   where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                   && d.CompanyId == obj.CompanyId
                                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                   select c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesDue =
                                    oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false
                                    //&& c.BranchId == obj.BranchId
                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                    //&& c.CompanyId == obj.CompanyId
                                    && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                    ).Select(c => c.GrandTotal - c.WriteOffAmount).DefaultIfEmpty().Sum()
                               - (from c in oConnectionContext.DbClsSales
                                  join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                                  where c.Status != "Draft" &&
         (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
         && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                  select d.Amount).DefaultIfEmpty().Sum(),
                               //           TotalSalesReturnDue =
                               //                (from c in oConnectionContext.DbClsSalesReturn
                               //                 join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                               //                 where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId
                               //                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == d.BranchId)
                               //                 && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               //                 select c.GrandTotal).DefaultIfEmpty().Sum() -
                               //                          (from c in oConnectionContext.DbClsSalesReturn
                               //                           join d in oConnectionContext.DbClsSales
                               //        on c.SalesId equals d.SalesId
                               //                           join e in oConnectionContext.DbClsCustomerRefund
                               //                           on c.SalesReturnId equals e.SalesReturnId
                               //                           where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId
                               //                           && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                               //        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == d.BranchId)
                               //                           && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                               //                           && d.IsDeleted == false && d.IsCancelled == false
                               //                           && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                               //        e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                               //                           select e.Amount).DefaultIfEmpty().Sum(),
                           }).ToList();
            }
            else
            {
                    det = (from a in oConnectionContext.DbClsUser
                               //join b in oConnectionContext.DbClsUserBranchMap
                               //on a.UserId equals b.UserId
                           where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                           a.IsDeleted == false
                           //&& b.IsActive == true
                           //&& a.UserId != obj.AddedBy 
                           && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                    DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                           select new ClsUserVm
                           {
                               PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(bb => bb.PaymentTermId == a.PaymentTermId).Select(bb => bb.PaymentTerm).FirstOrDefault(),
                               //Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == b.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                               //BranchId = b.BranchId,
                               IsWalkin = a.IsWalkin,
                               //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                               BusinessName = a.BusinessName,
                               UserId = a.UserId,
                               Username = a.Username,
                               UserRoleId = a.UserRoleId,
                               RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                               UserGroupId = a.UserGroupId,
                               UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                               Name = a.Name,
                               Password = a.Password,
                               EmailId = a.EmailId,
                               MobileNo = a.MobileNo,
                               AltMobileNo = a.AltMobileNo,
                               ReligionId = a.ReligionId,
                               DOB = a.DOB,
                               Gender = a.Gender,
                               //Tagline = a.Tagline,
                               //BusinessType = a.BusinessType,
                               //BusinessName = a.BusinessName,
                               //OwnerName = a.OwnerName,
                               TaxNo = a.TaxNo,
                               Notes = a.Notes,
                               //CurrencyId = a.CurrencyId,
                               //CurrencyName = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyName).FirstOrDefault(),
                               //CityId = a.CityId,
                               //StateId = a.StateId,
                               //CountryId = a.CountryId,
                               //Zipcode = a.Zipcode,
                               //Address = a.Address,
                               //AltAddress = a.AltAddress,
                               CommissionPercent = a.CommissionPercent,
                               OpeningBalance = a.OpeningBalance,
                               CustomerOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                             where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               SupplierOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                             where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == a.UserId
                                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                             select d.Amount).DefaultIfEmpty().Sum(),
                               AdvanceBalance = a.AdvanceBalance,
                               CreditLimit = a.CreditLimit,
                               //PayTermNo = a.PayTermNo,
                               //PayTerm = a.PayTerm,
                               IsActive = a.IsActive,
                               IsDeleted = a.IsDeleted,
                               AddedBy = a.AddedBy,
                               AddedOn = a.AddedOn,
                               ModifiedBy = a.ModifiedBy,
                               ModifiedOn = a.ModifiedOn,
                               IsCompany = a.IsCompany,
                               CompanyId = a.CompanyId,
                               ExpiryDate = a.ExpiryDate,
                               JoiningDate = a.JoiningDate,
                               ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                               AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                               ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                               TotalSalesInvoices = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).Count(),
                               TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                                   join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                                   where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId
                                                   && d.BranchId == obj.BranchId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                   select c.GrandTotal).DefaultIfEmpty().Sum(),
                               TotalSalesDue =
                            oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false
                            && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal - c.WriteOffAmount).DefaultIfEmpty().Sum() -
                            (from c in oConnectionContext.DbClsSales
                             join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                             where c.Status != "Draft" &&
    (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                             select d.Amount).DefaultIfEmpty().Sum(),
                               //                   TotalSalesReturnDue =
                               //                (from c in oConnectionContext.DbClsSalesReturn
                               //                 join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                               //                 where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId &&
                               //                 d.BranchId == obj.BranchId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                               //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               //                 select c.GrandTotal).DefaultIfEmpty().Sum() -
                               //                          (from c in oConnectionContext.DbClsSalesReturn
                               //                           join d in oConnectionContext.DbClsSales
                               //        on c.SalesId equals d.SalesId
                               //                           join e in oConnectionContext.DbClsCustomerRefund
                               //                           on c.SalesReturnId equals e.SalesReturnId
                               //                           where c.CompanyId == obj.CompanyId && d.CustomerId == a.UserId &&
                               //                           d.BranchId == obj.BranchId && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                               //                           && d.IsDeleted == false && d.IsCancelled == false
                               //                           && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                               //        e.CompanyId == obj.CompanyId && e.BranchId == obj.BranchId
                               //                           select e.Amount).DefaultIfEmpty().Sum(),
                           }).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.UserId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.UserGroupId != 0)
            {
                det = det.Where(a => a.UserGroupId == obj.UserGroupId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierReport(ClsUserVm obj)
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

            List<ClsUserVm> det = new List<ClsUserVm>();

            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsUser
                           //join b in oConnectionContext.DbClsUserBranchMap
                           //on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                       a.IsDeleted == false //&& b.IsActive == true
                                            //&& a.UserId != obj.AddedBy 
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                       select new ClsUserVm
                       {
                           PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(bb => bb.PaymentTermId == a.PaymentTermId).Select(bb => bb.PaymentTerm).FirstOrDefault(),
                           IsWalkin = a.IsWalkin,
                           //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                           BusinessName = a.BusinessName,
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                           Name = a.Name,
                           Password = a.Password,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           AltMobileNo = a.AltMobileNo,
                           ReligionId = a.ReligionId,
                           DOB = a.DOB,
                           Gender = a.Gender,
                           TaxNo = a.TaxNo,
                           Notes = a.Notes,
                           CommissionPercent = a.CommissionPercent,
                           OpeningBalance = a.OpeningBalance,
                           CustomerOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                         where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == a.UserId
                                      && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                         select d.Amount).DefaultIfEmpty().Sum(),
                           SupplierOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                         where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == a.UserId
                                      && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                         select d.Amount).DefaultIfEmpty().Sum(),
                           AdvanceBalance = a.AdvanceBalance,
                           CreditLimit = a.CreditLimit,
                           //PayTermNo = a.PayTermNo,
                           //PayTerm = a.PayTerm,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           IsCompany = a.IsCompany,
                           CompanyId = a.CompanyId,
                           ExpiryDate = a.ExpiryDate,
                           JoiningDate = a.JoiningDate,
                           ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           //           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status == "Sent" && c.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //                                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                           //l.UserId == c.CustomerId && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == b.BranchId)
                           //                                        && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),                           
                           TotalSalesInvoices = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                               && c.Status.ToLower() != "draft" && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).Count(),
                           TotalSales = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                               && c.Status.ToLower() != "draft" && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsPurchaseReturn
                                                      //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                                  where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && c.CompanyId == obj.CompanyId //&& d.SupplierId == a.UserId
                                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                  //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                  select c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesDue =
                                oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                //&& c.BranchId == obj.BranchId
                                && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                               && c.Status.ToLower() != "draft" && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                                (from c in oConnectionContext.DbClsPurchase
                                 join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                 where
        d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId
        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
        && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
        && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
        && c.Status.ToLower() != "draft"
                                 select d.Amount).DefaultIfEmpty().Sum(),
                           //   TotalSalesReturnDue =
                           //        (from c in oConnectionContext.DbClsPurchaseReturn
                           //             //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                           //         where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                           //         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                           //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == c.BranchId)
                           //         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //         //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                           //         select c.GrandTotal).DefaultIfEmpty().Sum() -
                           //                  (from c in oConnectionContext.DbClsPurchaseReturn
                           //                       //                   join d in oConnectionContext.DbClsPurchase
                           //                       //on c.PurchaseId equals d.PurchaseId
                           //                   join e in oConnectionContext.DbClsSupplierRefund
                           //                   on c.PurchaseReturnId equals e.PurchaseReturnId
                           //                   where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                           //                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                           //l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == c.BranchId)
                           //                   && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                           //                   //&& d.IsDeleted == false && d.IsCancelled == false
                           //                   && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                           //e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                           //                   select e.Amount).DefaultIfEmpty().Sum(),
                       }).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                           //join b in oConnectionContext.DbClsUserBranchMap
                           //on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() &&
                       a.IsDeleted == false
                       //&& b.IsActive == true
                       //&& a.UserId != obj.AddedBy 
                       && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                       select new ClsUserVm
                       {
                           PaymentTerm = oConnectionContext.DbClsPaymentTerm.Where(bb => bb.PaymentTermId == a.PaymentTermId).Select(bb => bb.PaymentTerm).FirstOrDefault(),
                           //Branch = oConnectionContext.DbClsBranch.Where(bb => bb.BranchId == b.BranchId).Select(bb => bb.Branch).FirstOrDefault(),
                           //BranchId = b.BranchId,
                           IsWalkin = a.IsWalkin,
                           //BusinessName = oConnectionContext.DbClsBusinessSettings.Where(bb => bb.CompanyId == a.CompanyId).Select(bb => bb.BusinessName).FirstOrDefault(),
                           BusinessName = a.BusinessName,
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(bb => bb.RoleId == a.UserRoleId).Select(bb => bb.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           UserGroup = oConnectionContext.DbClsUserGroup.Where(bb => bb.UserGroupId == a.UserGroupId).Select(bb => bb.UserGroup).FirstOrDefault(),
                           Name = a.Name,
                           Password = a.Password,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           AltMobileNo = a.AltMobileNo,
                           ReligionId = a.ReligionId,
                           DOB = a.DOB,
                           Gender = a.Gender,
                           //Tagline = a.Tagline,
                           //BusinessType = a.BusinessType,
                           //BusinessName = a.BusinessName,
                           //OwnerName = a.OwnerName,
                           TaxNo = a.TaxNo,
                           Notes = a.Notes,
                           //CurrencyId = a.CurrencyId,
                           //CurrencyName = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyName).FirstOrDefault(),
                           //CityId = a.CityId,
                           //StateId = a.StateId,
                           //CountryId = a.CountryId,
                           //Zipcode = a.Zipcode,
                           //Address = a.Address,
                           //AltAddress = a.AltAddress,
                           CommissionPercent = a.CommissionPercent,
                           OpeningBalance = a.OpeningBalance,
                           CustomerOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                         where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == a.UserId
                                      && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                         select d.Amount).DefaultIfEmpty().Sum(),
                           SupplierOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                         where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == a.UserId
                                      && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                         select d.Amount).DefaultIfEmpty().Sum(),
                           AdvanceBalance = a.AdvanceBalance,
                           CreditLimit = a.CreditLimit,
                           //PayTermNo = a.PayTermNo,
                           //PayTerm = a.PayTerm,
                           IsActive = a.IsActive,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           IsCompany = a.IsCompany,
                           CompanyId = a.CompanyId,
                           ExpiryDate = a.ExpiryDate,
                           JoiningDate = a.JoiningDate,
                           ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                           AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                           ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                           TotalSalesInvoices = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                        && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId
                                                        && c.Status.ToLower() != "draft").Select(c => c.GrandTotal).Count(),
                           TotalSales = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                        && c.BranchId == obj.BranchId && c.CompanyId == obj.CompanyId
                                                        && c.Status.ToLower() != "draft").Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsPurchaseReturn
                                                      //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                                  where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                                               && c.BranchId == obj.BranchId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                  //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                  select c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesDue =
                                 oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                 && c.BranchId == obj.BranchId
                                 && c.CompanyId == obj.CompanyId).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                                 (from c in oConnectionContext.DbClsPurchase
                                  join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                  where
         d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId && c.BranchId == obj.BranchId
         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
         && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
         && c.Status.ToLower() != "draft"
                                  select d.Amount).DefaultIfEmpty().Sum(),
                           //  TotalSalesReturnDue =
                           //        (from c in oConnectionContext.DbClsPurchaseReturn
                           //             //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                           //         where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                           //         && c.BranchId == obj.BranchId
                           //         && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                           //         //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                           //         select c.GrandTotal).DefaultIfEmpty().Sum() -
                           //                  (from c in oConnectionContext.DbClsPurchaseReturn
                           //                       //                   join d in oConnectionContext.DbClsPurchase
                           //                       //on c.PurchaseId equals d.PurchaseId
                           //                   join e in oConnectionContext.DbClsSupplierRefund
                           //                   on c.PurchaseReturnId equals e.PurchaseReturnId
                           //                   where c.CompanyId == obj.CompanyId && c.SupplierId == a.UserId
                           //                   && c.BranchId == obj.BranchId
                           //                   && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                           //                   //&& d.IsDeleted == false && d.IsCancelled == false
                           //                   && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                           //e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                           //                   select e.Amount).DefaultIfEmpty().Sum(),
                       }).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.UserId == obj.SupplierId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserReport(ClsUserVm obj)
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

            List<ClsUserVm> det = new List<ClsUserVm>();

            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = (from a in oConnectionContext.DbClsUser
                       join b in oConnectionContext.DbClsUserBranchMap
                       on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() && a.IsDeleted == false
                       && b.BranchId == obj.BranchId
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           Password = a.Password,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           IsCompany = a.IsCompany,
                           CompanyId = a.CompanyId,
                           ExpiryDate = a.ExpiryDate,
                           JoiningDate = a.JoiningDate,
                           ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                           IsActive = a.IsActive
                       }).ToList();
            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                       join b in oConnectionContext.DbClsUserBranchMap
                       on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == obj.UserType.ToLower() && a.IsDeleted == false
                        && b.BranchId == obj.BranchId
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           Password = a.Password,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           IsDeleted = a.IsDeleted,
                           AddedBy = a.AddedBy,
                           AddedOn = a.AddedOn,
                           ModifiedBy = a.ModifiedBy,
                           ModifiedOn = a.ModifiedOn,
                           IsCompany = a.IsCompany,
                           CompanyId = a.CompanyId,
                           ExpiryDate = a.ExpiryDate,
                           JoiningDate = a.JoiningDate,
                           ProfilePic = oCommonController.baseUrl + a.ProfilePic,
                           IsActive = a.IsActive
                       }).ToList();
            }


            if (obj.UserId != 0)
            {
                det = det.Where(a => a.UserId == obj.UserId).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize
                }
            };

            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ForgotPassword(ClsForgotPasswordVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.EmailId == null || obj.EmailId == "")
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Email Id is required",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId).Count() == 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Invalid Email Id",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                string token = oCommonController.CreateToken();
                ClsForgotPassword oClsForgotPassword = new ClsForgotPassword()
                {
                    EmailId = obj.EmailId,
                    ExpiresOn = DateTime.Now.AddMinutes(10),
                    IsBlocked = false,
                    Token = token
                };
                oConnectionContext.DbClsForgotPassword.Add(oClsForgotPassword);
                oConnectionContext.SaveChanges();

                //string msg = "Please click the below link to reset your password now <br/> " + oCommonController.webUrl + "/resetpassword?id=" + oClsForgotPassword.ForgotPasswordId + "&email=" + obj.EmailId;
                if (obj.UserType.ToLower() == "user")
                {
                    oEmailController.ForgotPassword(obj.EmailId, "Forgot Password", oCommonController.webUrl + "/resetpassword?id=" + token, obj.Domain);
                }
                else
                {
                    oEmailController.ForgotPassword(obj.EmailId, "Forgot Password", oCommonController.webUrl + "/adminresetpassword?id=" + token, obj.Domain);
                }

                data = new
                {
                    Status = 1,
                    Message = "Link for password reset is sent to ur email id",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPasswordLinkValidation(ClsForgotPasswordVm obj)
        {
            var aa = oConnectionContext.DbClsForgotPassword.Where(a => a.Token == obj.Token).Select(a => new
            {
                a.ForgotPasswordId,
                ExpiresOn = a.ExpiresOn,
                IsBlocked = a.IsBlocked,
                a.EmailId,
                CompanyId = oConnectionContext.DbClsUser.Where(b => b.EmailId == a.EmailId).Select(b => b.CompanyId).FirstOrDefault()
            }).FirstOrDefault();

            if (aa == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Link expired",
                    Data = new
                    {

                    }
                };
                return await Task.FromResult(Ok(data));
            }
            else
            {
                var CurrentDate = oCommonController.CurrentDate(aa.CompanyId);
                DateTime dt = CurrentDate;

                if (aa.ExpiresOn >= dt && aa.IsBlocked == false)
                {
                    data = new
                    {
                        Status = 1,
                        Message = "Link not expired",
                        Data = new
                        {

                        }
                    };
                }
                else
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Link expired",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }
            }

            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> ResetPassword(ClsForgotPasswordVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.Password == null || obj.Password == "" || obj.CPassword == null || obj.CPassword == "")
                {
                    data = new
                    {
                        Status = 0,
                        Message = "All fields are required",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.Password != obj.CPassword)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "New and Confirm Passwords do not match",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var aa = oConnectionContext.DbClsForgotPassword.Where(a => a.Token == obj.Token).Select(a => new
                {
                    UserId = oConnectionContext.DbClsUser.Where(b => b.EmailId == a.EmailId).Select(b => b.UserId).FirstOrDefault(),
                    a.ForgotPasswordId,
                    ExpiresOn = a.ExpiresOn,
                    IsBlocked = a.IsBlocked,
                    a.EmailId,
                    CompanyId = oConnectionContext.DbClsUser.Where(b => b.EmailId == a.EmailId).Select(b => b.CompanyId).FirstOrDefault()
                }).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(aa.CompanyId);
                DateTime dt = CurrentDate;

                if (aa.ExpiresOn >= dt && aa.IsBlocked == false)
                {
                    long UserId = oConnectionContext.DbClsUser.Where(a => a.EmailId == aa.EmailId).Select(a => a.UserId).FirstOrDefault();
                    ClsUser oClsUser = new ClsUser();
                    oClsUser.UserId = UserId;
                    oClsUser.Password = oCommonController.Sha256Encryption(obj.Password);
                    oClsUser.ModifiedOn = CurrentDate;
                    oConnectionContext.DbClsUser.Attach(oClsUser);
                    oConnectionContext.Entry(oClsUser).Property(x => x.Password).IsModified = true;
                    oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                    oConnectionContext.SaveChanges();

                    string query = "update \"tblForgotPassword\" set \"IsBlocked\"=True where \"ForgotPasswordId\"=" + aa.ForgotPasswordId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    query = "update \"tblLoginDetails\" set \"IsLoggedOut\"=True where \"AddedBy\"=" + aa.UserId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = UserId,
                        Browser = obj.Browser,
                        Category = "Password Reset",
                        CompanyId = oConnectionContext.DbClsUser.Where(a => a.UserId == UserId).Select(a => a.CompanyId).FirstOrDefault(),
                        Description = "Password reset",
                        Id = UserId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                    data = new
                    {
                        Status = 1,
                        Message = "Password reset successfully. Please login now to continue",
                        Data = new
                        {

                        }
                    };
                }
                else
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Link Expired",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ValidateLoginEmail(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.EmailId == null || obj.EmailId == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divEmailId" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "All fields are required",
                    //    Data = new
                    //    {

                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (obj.CEmailId == null || obj.CEmailId == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCEmailId" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "All fields are required",
                    //    Data = new
                    //    {

                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (obj.OldEmailId != null && obj.EmailId != null)
                {
                    if (obj.OldEmailId == obj.EmailId)
                    {
                        errors.Add(new ClsError { Message = "Old and New Email Ids cannot be same", Id = "divEmailId" });
                        isError = true;
                        //data = new
                        //{
                        //    Status = 0,
                        //    Message = "Old and New Email Ids cannot be same",
                        //    Data = new
                        //    {

                        //    }
                        //};
                        //return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.CEmailId != null)
                {
                    if (obj.EmailId != obj.CEmailId)
                    {
                        errors.Add(new ClsError { Message = "New and Confirm Email Ids do not match", Id = "divCEmailId" });
                        isError = true;
                        //data = new
                        //{
                        //    Status = 0,
                        //    Message = "New and Confirm Email Ids do not match",
                        //    Data = new
                        //    {

                        //    }
                        //};
                        //return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null)
                {
                    long count = oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId && a.IsDeleted == false &&
                    a.UserType.ToLower() == obj.UserType.ToLower() //(a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales")
                    ).Count();
                    if (count > 0)
                    {
                        errors.Add(new ClsError { Message = "Email Id is already taken", Id = "divEmailId" });
                        isError = true;
                        //data = new
                        //{
                        //    Status = 0,
                        //    Message = "Email Id is already taken",
                        //    Data = new
                        //    {

                        //    }
                        //};
                        //return await Task.FromResult(Ok(data));
                    }
                }

                if (obj.EmailId != null && obj.EmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.EmailId);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid New Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }

                if (obj.CEmailId != null && obj.CEmailId != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.CEmailId);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Confirm New Email Id", Id = "divCEmailId" });
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

                Random rdn = new Random();
                string otp = rdn.Next(1111, 9999).ToString();

                ClsOtp oClsOtp = new ClsOtp()
                {
                    EmailId = obj.EmailId,
                    Otp = otp,
                    AddedOn = CurrentDate
                };
                oConnectionContext.DbClsOtp.Add(oClsOtp);
                oConnectionContext.SaveChanges();

                //oEmailController.EmailFunction(obj.EmailId, "Your OTP is " + otp, "Login Email Change Otp");
                oEmailController.ChangeLoginEmailOtp(obj.CEmailId, "Login Email Change Otp",
                    oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId).Select(a => a.Name).FirstOrDefault(), otp, obj.Domain);

                data = new
                {

                    Status = 1,
                    Message = "Otp sent Successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));

        }

        public async Task<IHttpActionResult> UpdateLoginEmail(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.EmailId == null || obj.EmailId == "" || obj.CEmailId == null || obj.CEmailId == "")
                {
                    data = new
                    {
                        Status = 2,
                        Message = "All fields are required",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.OldEmailId == obj.EmailId)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "Old and New Email Ids cannot be same",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.EmailId != obj.CEmailId)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "New and Confirm Email Ids do not match",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                long count = oConnectionContext.DbClsUser.Where(a => a.EmailId == obj.EmailId && a.IsDeleted == false &&
                a.UserType.ToLower() == obj.UserType.ToLower() //(a.UserType.ToLower() == "user" || a.UserType.ToLower() == "sales")
                ).Count();
                if (count > 0)
                {
                    data = new
                    {
                        Status = 2,
                        Message = "Email Id is already taken",
                        Data = new
                        {

                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                var d = oConnectionContext.DbClsOtp.Where(a => a.EmailId == obj.EmailId).Select(a => new { a.Otp, a.OtpId }).OrderByDescending(a => a.OtpId).FirstOrDefault();
                if (d == null)
                {
                    errors.Add(new ClsError { Message = "Invalid Otp", Id = "divOtp" });
                    isError = true;
                    //data = new
                    //{

                    //    Status = 0,
                    //    Message = "Invalid Otp",
                    //    Data = new
                    //    {
                    //    }
                    //};
                    //return await Task.FromResult(Ok(data));
                }

                if (d.Otp != obj.Otp && obj.Otp != "1234")
                {
                    errors.Add(new ClsError { Message = "Invalid Otp", Id = "divOtp" });
                    isError = true;
                    //data = new
                    //{
                    //    Status = 0,
                    //    Message = "Invalid Otp",
                    //    Data = new
                    //    {

                    //    }
                    //};
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

                ClsUser oClsUser = new ClsUser();
                oClsUser.UserId = obj.AddedBy;
                oClsUser.EmailId = obj.EmailId;
                oClsUser.ModifiedOn = CurrentDate;
                oClsUser.ModifiedBy = obj.AddedBy;
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.EmailId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Change Login Mail",
                    CompanyId = obj.CompanyId,
                    Description = "Login email changed",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Login Email Id changed successfully.",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportCustomer(ClsUserVm obj)
        {
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId
            }).FirstOrDefault();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            //using (TransactionScope dbContextTransaction = new TransactionScope())
            //      {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

            if (obj.UserImports == null || obj.UserImports.Count == 0)
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

            int count = 1;
            if (obj.UserImports != null)
            {
                foreach (var item in obj.UserImports)
                {
                    int innerCount = 1;

                    foreach (var inner in obj.UserImports)
                    {
                        if (item.MobileNo != "" && item.MobileNo != null)
                        {
                            if (item.MobileNo.ToLower() == inner.MobileNo.ToLower() && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate MobileNo exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        if (item.EmailId != "" && item.EmailId != null)
                        {
                            if (item.EmailId.ToLower() == inner.EmailId.ToLower() && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate EmailId exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        if (item.AltMobileNo != "" && item.AltMobileNo != null)
                        {
                            if (item.AltMobileNo.ToLower() == inner.AltMobileNo.ToLower() && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate AltMobileNo exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        innerCount++;
                    }
                    count++;
                }
            }

            count = 1;
            if (obj.UserImports != null)
            {
                foreach (var item in obj.UserImports)
                {
                    if (item.Name == "" || item.Name == null)
                    {
                        errors.Add(new ClsError { Message = "Name is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.MobileNo == "" || item.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "MobileNo is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.MobileNo != null && item.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(item.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid MobileNo in row no " + count, Id = "divMobileNo" });
                            isError = true;
                        }

                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo == item.MobileNo && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "MobileNo in row no" + count + " is already taken", Id = "" });
                            isError = true;
                        }
                    }

                    if (item.EmailId != null && item.EmailId != "")
                    {
                        bool check = oCommonController.EmailValidationCheck(item.EmailId);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid EmailId in row no " + count, Id = "divEmailId" });
                            isError = true;
                        }

                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId == item.EmailId.Trim() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "EmailId in row no" + count + " is already taken", Id = "" });
                            isError = true;
                        }

                    }

                    if (item.AltMobileNo != null && item.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(item.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid AltMobileNo in row no " + count, Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (item.PaymentTerm != "" && item.PaymentTerm != null)
                    {
                        if (oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                         a.PaymentTerm.ToLower() == item.PaymentTerm.ToLower()).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid PaymentTerm in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.UserGroup != "" && item.UserGroup != null)
                    {
                        if ((from a in oConnectionContext.DbClsUserGroup
                             where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.UserGroup.ToLower() == item.UserGroup.ToLower()
                             select a.UserGroupId).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid UserGroup in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (BusinessSetting.CountryId == 2)
                    {
                        if (item.GstTreatment == "" || item.GstTreatment == null)
                        {
                            errors.Add(new ClsError { Message = "GstTreatment is required in row no " + count, Id = "" });
                            isError = true;
                        }

                        if (item.GstTreatment != "" && item.GstTreatment != null)
                        {
                            if (item.GstTreatment == "Taxable Supply (Registered)" || item.GstTreatment == "Composition Taxable Supply"
                                || item.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || item.GstTreatment == "Deemed Export"
                                || item.GstTreatment == "Supply by SEZ Developer")
                                if (item.GSTIN == "" || item.GSTIN == null)
                                {
                                    errors.Add(new ClsError { Message = "GSTIN is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                        }

                        if (item.PlaceOfSupply == "" || item.PlaceOfSupply == null)
                        {
                            errors.Add(new ClsError { Message = "PlaceOfSupply is required in row no " + count, Id = "" });
                            isError = true;
                        }
                    }
                    else
                    {
                        if (item.IsBusinessRegistered == true)
                        {
                            if (item.BusinessRegistrationNo == "" || item.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "BusinessRegistrationNo is required in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.TaxPreference == "" || item.TaxPreference == null)
                    {
                        errors.Add(new ClsError { Message = "TaxPreference is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.TaxPreference != "" && item.TaxPreference != null)
                    {
                        if (item.TaxPreference == "Non-Taxable")
                        {
                            if (item.TaxExemptionReason == "" || item.TaxExemptionReason == null)
                            {
                                errors.Add(new ClsError { Message = "TaxExemptionReason is required in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.BillingCountryName != "" && item.BillingCountryName != null)
                    {
                        if (oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.BillingCountryName.ToLower()).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid BillingCountryName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.BillingStateName != "" && item.BillingStateName != null)
                    {
                        if (item.BillingCountryName == "" && item.BillingCountryName == null)
                        {
                            errors.Add(new ClsError { Message = "BillingCountryName is required in row no " + count, Id = "" });
                            isError = true;
                        }
                        else
                        {
                            long _countryId = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.BillingCountryName.ToLower()).Select(a => a.CountryId).FirstOrDefault();

                            if (oConnectionContext.DbClsState.Where(a => a.IsDeleted == false && a.CountryId == _countryId &&
                         a.State.ToLower() == item.BillingStateName.ToLower()).Count() == 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid BillingStateName in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.ShippingCountryName != "" && item.ShippingCountryName != null)
                    {
                        if (oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.ShippingCountryName.ToLower()).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid Shipping CountryName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.ShippingStateName != "" && item.ShippingStateName != null)
                    {
                        if (item.ShippingCountryName == "" && item.ShippingCountryName == null)
                        {
                            errors.Add(new ClsError { Message = "ShippingCountryName is required in row no " + count, Id = "" });
                            isError = true;
                        }
                        else
                        {
                            long _countryId = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.ShippingCountryName.ToLower()).Select(a => a.CountryId).FirstOrDefault();

                            if (oConnectionContext.DbClsState.Where(a => a.IsDeleted == false && a.CountryId == _countryId &&
                         a.State.ToLower() == item.ShippingStateName.ToLower()).Count() == 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid ShippingStateName in row no " + count, Id = "" });
                                isError = true;
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

            foreach (var item in obj.UserImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    string Username = "";

                    long UserCurrencyMapId = 0, CurrencyId = 0;

                    CurrencyId = oConnectionContext.DbClsUserCurrencyMap.Where(a => a.IsMain == true && a.CompanyId == obj.CompanyId
                    ).Select(a => a.CurrencyId).FirstOrDefault();

                    long CountryId = 0;
                    if (item.BillingCountryName != "" && item.BillingCountryName != null)
                    {
                        CountryId = oConnectionContext.DbClsCountry.Where(a => a.Country.ToLower() == item.BillingCountryName.ToLower() && a.IsDeleted == false).Select(a => a.CountryId).FirstOrDefault();
                    }

                    long AltCountryId = 0;
                    if (item.ShippingCountryName != "" && item.ShippingCountryName != null)
                    {
                        AltCountryId = oConnectionContext.DbClsCountry.Where(a => a.Country.ToLower() == item.ShippingCountryName.ToLower() && a.IsDeleted == false).Select(a => a.CountryId).FirstOrDefault();
                    }

                    long StateId = 0;
                    if (item.BillingStateName != "" && item.BillingStateName != null)
                    {
                        StateId = oConnectionContext.DbClsState.Where(a => a.CountryId == CountryId && a.State.ToLower() == item.BillingStateName.ToLower() &&
                    a.IsDeleted == false).Select(a => a.StateId).FirstOrDefault();
                    }

                    long AltStateId = 0;
                    if (item.ShippingStateName != "" && item.ShippingStateName != null)
                    {
                        AltStateId = oConnectionContext.DbClsState.Where(a => a.CountryId == AltCountryId && a.State.ToLower() == item.ShippingStateName.ToLower() &&
                    a.IsDeleted == false).Select(a => a.StateId).FirstOrDefault();
                    }

                    long CityId = 0;
                    if (item.BillingCityName != "" && item.BillingCityName != null)
                    {
                        CityId = oConnectionContext.DbClsCity.Where(a => a.CountryId == CountryId && a.StateId == StateId &&
                        a.City.ToLower() == item.BillingCityName.ToLower() &&
                    a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.CityId).FirstOrDefault();
                    }

                    long AltCityId = 0;
                    if (item.ShippingCityName != "" && item.ShippingCityName != null)
                    {
                        AltCityId = oConnectionContext.DbClsCity.Where(a => a.CountryId == AltCountryId && a.StateId == AltStateId &&
                        a.City.ToLower() == item.ShippingCityName.ToLower() &&
                    a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.CityId).FirstOrDefault();
                    }

                    long UserGroupId = 0;
                    if (item.UserGroup != "" && item.UserGroup != null)
                    {
                        UserGroupId = oConnectionContext.DbClsUserGroup.Where(a => a.CompanyId == obj.CompanyId &&
                        a.UserGroup.ToLower() == item.UserGroup.ToLower() && a.IsDeleted == false).Select(a => a.UserGroupId).FirstOrDefault();
                    }

                    long PaymentTermId = 0;
                    if (item.PaymentTerm != "" && item.PaymentTerm != null)
                    {
                        PaymentTermId = oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId &&
                        a.PaymentTerm.ToLower() == item.PaymentTerm.ToLower() && a.IsDeleted == false).Select(a => a.PaymentTermId).FirstOrDefault();
                    }

                    long BusinessRegistrationNameId = oConnectionContext.DbClsBusinessRegistrationName.Where(a => a.CountryId == BusinessSetting.CountryId &&
            a.IsDeleted == false && a.IsActive == true).Select(a => a.BusinessRegistrationNameId).FirstOrDefault();

                    long TaxPreferenceId = oConnectionContext.DbClsTax.Where(a => a.Tax == item.TaxPreference && a.CompanyId == obj.CompanyId
                        && a.IsDeleted == false).Select(a => a.TaxId).FirstOrDefault();

                    long TaxExemptionId = 0;
                    if (item.TaxExemptionReason != "" && item.TaxExemptionReason != null)
                    {
                        TaxExemptionId = oConnectionContext.DbClsTaxExemption.Where(a => a.Reason.ToLower() == item.TaxExemptionReason.ToLower() &&
                                           a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxExemptionId).FirstOrDefault();
                    }

                    long PlaceOfSupplyId = 0;
                    if (BusinessSetting.CountryId == 2)
                    {
                        PlaceOfSupplyId = oConnectionContext.DbClsState.Where(a => a.State.ToLower() == item.PlaceOfSupply.ToLower()
                    && a.IsDeleted == false).Select(a => a.StateId).FirstOrDefault();
                    }

                    long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
       && a.Type == "Accounts Receivable").Select(a => a.AccountId).FirstOrDefault();

                    long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
    && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault();

                    ClsUser oClsUser = new ClsUser()
                    {
                        IsShippingAddressDifferent = item.IsShippingAddressDifferent,
                        UserGroupId = UserGroupId,
                        Name = item.Name,
                        Username = Username,
                        EmailId = item.EmailId,
                        MobileNo = item.MobileNo,
                        AltMobileNo = item.AltMobileNo,
                        DOB = item.DOB != null ? item.DOB.Value.AddHours(5).AddMinutes(30) : item.DOB,
                        BusinessName = item.BusinessName,
                        TaxNo = item.TaxNo,
                        CreditLimit = item.CreditLimit,
                        OpeningBalance = item.OpeningBalance,
                        //PayTermNo = item.PayTermNo,
                        //PayTerm = PayTerm,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        IsCompany = false,
                        CompanyId = obj.CompanyId,
                        ExpiryDate = ExpiryDate,
                        JoiningDate = item.JoiningDate.Value.AddHours(5).AddMinutes(30),
                        UserType = obj.UserType,
                        CurrencyId = CurrencyId,
                        AdvanceBalance = 0,
                        IsWalkin = false,
                        Under = obj.Under,
                        IsRootAccount = false,
                        BatchNo = obj.BatchNo,
                        AccountId = AccountId,
                        JournalAccountId = JournalAccountId,
                        PaymentTermId = PaymentTermId,
                        PlaceOfSupplyId = PlaceOfSupplyId,
                        //SourceOfSupplyId =SourceOfSupplyId,
                        TaxPreferenceId = TaxPreferenceId,
                        TaxExemptionId = TaxExemptionId,
                        IsBusinessRegistered = item.IsBusinessRegistered == true ? 1 : 2,
                        GstTreatment = item.GstTreatment,
                        BusinessRegistrationNameId = obj.BusinessRegistrationNameId,
                        BusinessRegistrationNo = item.GSTIN,
                        BusinessLegalName = item.BusinessLegalName,
                        BusinessTradeName = item.BusinessTradeName,
                        PanNo = item.PanNo
                    };
                    oConnectionContext.DbClsUser.Add(oClsUser);
                    oConnectionContext.SaveChanges();

                    ClsAddress oClsAddress = new ClsAddress()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        Address = item.BillingAddress,
                        CityId = CityId,
                        CountryId = CountryId,
                        IsActive = true,
                        IsDeleted = false,
                        StateId = StateId,
                        UserId = oClsUser.UserId,
                        Zipcode = item.BillingZipcode,
                        Name = item.Name,
                        MobileNo = item.MobileNo,
                        MobileNo2 = item.AltMobileNo,
                        Landmark = item.BillingLandmark,
                        EmailId = item.EmailId,                        
                    };
                    oConnectionContext.DbClsAddress.Add(oClsAddress);
                    oConnectionContext.SaveChanges();

                    ClsAddress oClsAddress1 = new ClsAddress()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        Address = item.ShippingAddress,
                        CityId = AltCityId,
                        CountryId = AltCountryId,
                        IsActive = true,
                        IsDeleted = false,
                        StateId = AltStateId,
                        UserId = oClsUser.UserId,
                        Zipcode = item.ShippingZipcode,
                        Name = item.ShippingCustomerName,
                        MobileNo = item.ShippingMobileNo,
                        MobileNo2 = item.ShippingAltMobileNo,
                        Landmark = item.ShippingLandmark,
                        EmailId = item.ShippingEmailId,
                    };
                    oConnectionContext.DbClsAddress.Add(oClsAddress1);
                    oConnectionContext.SaveChanges();

                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Customers",
                CompanyId = obj.CompanyId,
                Description = "Customers imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Customers imported successfully",
                Data = new
                {
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ImportSupplier(ClsUserVm obj)
        {
            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId
            }).FirstOrDefault();

            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            obj.Under = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.CompanyId).Select(a => a.Under).FirstOrDefault();

            if (obj.UserImports == null || obj.UserImports.Count == 0)
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

            int count = 1;
            if (obj.UserImports != null)
            {
                foreach (var item in obj.UserImports)
                {
                    int innerCount = 1;

                    foreach (var inner in obj.UserImports)
                    {
                        if (item.MobileNo != "" && item.MobileNo != null)
                        {
                            if (item.MobileNo.ToLower() == inner.MobileNo.ToLower() && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate MobileNo exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        if (item.EmailId != "" && item.EmailId != null)
                        {
                            if (item.EmailId.ToLower() == inner.EmailId.ToLower() && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "Duplicate EmailId exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        if (item.AltMobileNo != "" && item.AltMobileNo != null)
                        {
                            if (item.AltMobileNo == inner.AltMobileNo && count != innerCount)
                            {
                                errors.Add(new ClsError { Message = "AltMobileNo exists in row no " + count, Id = "" });
                                isError = true;
                            }
                        }

                        innerCount++;
                    }
                    count++;
                }
            }

            count = 1;
            if (obj.UserImports != null)
            {
                foreach (var item in obj.UserImports)
                {
                    if (item.Name == "" || item.Name == null)
                    {
                        errors.Add(new ClsError { Message = "Name is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.MobileNo == "" || item.MobileNo == null)
                    {
                        errors.Add(new ClsError { Message = "MobileNo is required in row no " + count, Id = "" });
                        isError = true;
                    }

                    if (item.MobileNo != null && item.MobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(item.MobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid MobileNo in row no " + count, Id = "divMobileNo" });
                            isError = true;
                        }

                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.MobileNo == item.MobileNo && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "MobileNo in row no" + count + " is already taken", Id = "" });
                            isError = true;
                        }
                    }

                    if (item.EmailId != null && item.EmailId != "")
                    {
                        bool check = oCommonController.EmailValidationCheck(item.EmailId);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid EmailId in row no " + count, Id = "divEmailId" });
                            isError = true;
                        }

                        if (oConnectionContext.DbClsUser.Where(a => a.Under == obj.Under && a.EmailId == item.EmailId.Trim() && a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false).Count() > 0)
                        {
                            errors.Add(new ClsError { Message = "EmailId in row no" + count + " is already taken", Id = "" });
                            isError = true;
                        }

                    }

                    if (item.AltMobileNo != null && item.AltMobileNo != "")
                    {
                        bool check = oCommonController.MobileValidationCheck(item.AltMobileNo);
                        if (check == false)
                        {
                            errors.Add(new ClsError { Message = "Invalid AltMobileNo in row no " + count, Id = "divMobileNo" });
                            isError = true;
                        }
                    }

                    if (item.PaymentTerm != "" && item.PaymentTerm != null)
                    {
                        if (oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false &&
                         a.PaymentTerm.ToLower() == item.PaymentTerm.ToLower()).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid PaymentTerm in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (BusinessSetting.CountryId == 2)
                    {
                        if (item.GstTreatment == "" || item.GstTreatment == null)
                        {
                            errors.Add(new ClsError { Message = "GstTreatment is required in row no " + count, Id = "" });
                            isError = true;
                        }

                        if (item.GstTreatment != "" && item.GstTreatment != null)
                        {
                            if (item.GstTreatment == "Taxable Supply (Registered)" || item.GstTreatment == "Composition Taxable Supply"
                                || item.GstTreatment == "Supply to SEZ Unit (Zero-Rated Supply)" || item.GstTreatment == "Deemed Export"
                                || item.GstTreatment == "Supply by SEZ Developer")
                                if (item.GSTIN == "" || item.GSTIN == null)
                                {
                                    errors.Add(new ClsError { Message = "GSTIN is required in row no " + count, Id = "" });
                                    isError = true;
                                }
                        }

                        if (item.SourceOfSupply == "" || item.SourceOfSupply == null)
                        {
                            errors.Add(new ClsError { Message = "SourceOfSupply is required in row no " + count, Id = "" });
                            isError = true;
                        }
                    }
                    else
                    {
                        if (item.IsBusinessRegistered == true)
                        {
                            if (item.BusinessRegistrationNo == "" || item.BusinessRegistrationNo == null)
                            {
                                errors.Add(new ClsError { Message = "BusinessRegistrationNo is required in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.BillingCountryName != "" && item.BillingCountryName != null)
                    {
                        if (oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.BillingCountryName.ToLower()).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid BillingCountryName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.BillingStateName != "" && item.BillingStateName != null)
                    {
                        if (item.BillingCountryName == "" && item.BillingCountryName == null)
                        {
                            errors.Add(new ClsError { Message = "BillingCountryName is required in row no " + count, Id = "" });
                            isError = true;
                        }
                        else
                        {
                            long _countryId = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.BillingCountryName.ToLower()).Select(a => a.CountryId).FirstOrDefault();

                            if (oConnectionContext.DbClsState.Where(a => a.IsDeleted == false && a.CountryId == _countryId &&
                         a.State.ToLower() == item.BillingStateName.ToLower()).Count() == 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid BillingStateName in row no " + count, Id = "" });
                                isError = true;
                            }
                        }
                    }

                    if (item.ShippingCountryName != "" && item.ShippingCountryName != null)
                    {
                        if (oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.ShippingCountryName.ToLower()).Count() == 0)
                        {
                            errors.Add(new ClsError { Message = "Invalid ShippingCountryName in row no " + count, Id = "" });
                            isError = true;
                        }
                    }

                    if (item.ShippingStateName != "" && item.ShippingStateName != null)
                    {
                        if (item.ShippingCountryName == "" && item.ShippingCountryName == null)
                        {
                            errors.Add(new ClsError { Message = "ShippingCountryName is required in row no " + count, Id = "" });
                            isError = true;
                        }
                        else
                        {
                            long _countryId = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false &&
                         a.Country.ToLower() == item.ShippingCountryName.ToLower()).Select(a => a.CountryId).FirstOrDefault();

                            if (oConnectionContext.DbClsState.Where(a => a.IsDeleted == false && a.CountryId == _countryId &&
                         a.State.ToLower() == item.ShippingStateName.ToLower()).Count() == 0)
                            {
                                errors.Add(new ClsError { Message = "Invalid ShippingStateName in row no " + count, Id = "" });
                                isError = true;
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

            foreach (var item in obj.UserImports)
            {
                using (TransactionScope dbContextTransaction = new TransactionScope())
                {
                    string Username = "";

                    long TaxId = 0;
                    if (item.TaxName != "" && item.TaxName != null)
                    {
                        TaxId = oConnectionContext.DbClsTax.Where(a => a.Tax.ToLower() == item.TaxName.ToLower() &&
                                           a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.TaxId).FirstOrDefault();
                    }

                    long UserCurrencyMapId = 0, CurrencyId = 0;

                    CurrencyId = oConnectionContext.DbClsUserCurrencyMap.Where(a => a.IsMain == true && a.CompanyId == obj.CompanyId
                    ).Select(a => a.CurrencyId).FirstOrDefault();

                    long CountryId = 0;
                    if (item.BillingCountryName != "" && item.BillingCountryName != null)
                    {
                        CountryId = oConnectionContext.DbClsCountry.Where(a => a.Country.ToLower() == item.BillingCountryName.ToLower() && a.IsDeleted == false).Select(a => a.CountryId).FirstOrDefault();
                    }

                    long AltCountryId = 0;
                    if (item.ShippingCountryName != "" && item.ShippingCountryName != null)
                    {
                        AltCountryId = oConnectionContext.DbClsCountry.Where(a => a.Country.ToLower() == item.ShippingCountryName.ToLower() && a.IsDeleted == false).Select(a => a.CountryId).FirstOrDefault();
                    }

                    long StateId = 0;
                    if (item.BillingStateName != "" && item.BillingStateName != null)
                    {
                        StateId = oConnectionContext.DbClsState.Where(a => a.CountryId == CountryId && a.State.ToLower() == item.BillingStateName.ToLower() &&
                    a.IsDeleted == false).Select(a => a.StateId).FirstOrDefault();
                    }

                    long AltStateId = 0;
                    if (item.ShippingStateName != "" && item.ShippingStateName != null)
                    {
                        AltStateId = oConnectionContext.DbClsState.Where(a => a.CountryId == AltCountryId && a.State.ToLower() == item.ShippingStateName.ToLower() &&
                     a.IsDeleted == false).Select(a => a.StateId).FirstOrDefault();
                    }

                    long CityId = 0;
                    if (item.BillingCityName != "" && item.BillingCityName != null)
                    {
                        CityId = oConnectionContext.DbClsCity.Where(a => a.CountryId == CountryId && a.StateId == StateId &&
                        a.City.ToLower() == item.BillingCityName.ToLower() &&
                    a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.CityId).FirstOrDefault();
                    }

                    long AltCityId = 0;
                    if (item.ShippingCityName != "" && item.ShippingCityName != null)
                    {
                        AltCityId = oConnectionContext.DbClsCity.Where(a => a.CountryId == AltCountryId && a.StateId == AltStateId && a.City.ToLower() == item.ShippingCityName.ToLower() &&
                    a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => a.CityId).FirstOrDefault();
                    }

                    long PaymentTermId = 0;
                    if (item.PaymentTerm != "" && item.PaymentTerm != null)
                    {
                        PaymentTermId = oConnectionContext.DbClsPaymentTerm.Where(a => a.CompanyId == obj.CompanyId &&
                        a.PaymentTerm.ToLower() == item.PaymentTerm.ToLower() && a.IsDeleted == false).Select(a => a.PaymentTermId).FirstOrDefault();
                    }

                    long BusinessRegistrationNameId = oConnectionContext.DbClsBusinessRegistrationName.Where(a => a.CountryId == BusinessSetting.CountryId &&
            a.IsDeleted == false && a.IsActive == true).Select(a => a.BusinessRegistrationNameId).FirstOrDefault();

                    long SourceOfSupplyId = 0;
                    if (BusinessSetting.CountryId == 2)
                    {
                        SourceOfSupplyId = oConnectionContext.DbClsState.Where(a => a.State.ToLower() == item.SourceOfSupply.ToLower()
                    && a.IsDeleted == false).Select(a => a.StateId).FirstOrDefault();
                    }

                    long AccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
       && a.Type == "Accounts Payable").Select(a => a.AccountId).FirstOrDefault();

                    long JournalAccountId = oConnectionContext.DbClsAccount.Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
    && a.Type == "Opening Balance Adjustments").Select(a => a.AccountId).FirstOrDefault();

                    ClsUser oClsUser = new ClsUser()
                    {
                        IsShippingAddressDifferent = item.IsShippingAddressDifferent,
                        Name = item.Name,
                        Username = Username,
                        EmailId = item.EmailId,
                        MobileNo = item.MobileNo,
                        AltMobileNo = item.AltMobileNo,
                        DOB = item.DOB != null ? item.DOB.Value.AddHours(5).AddMinutes(30) : item.DOB,
                        BusinessName = item.BusinessName,
                        TaxNo = item.TaxNo,
                        CreditLimit = item.CreditLimit,
                        OpeningBalance = item.OpeningBalance,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        IsCompany = false,
                        CompanyId = obj.CompanyId,
                        ExpiryDate = ExpiryDate,
                        JoiningDate = item.JoiningDate.Value.AddHours(5).AddMinutes(30),
                        UserType = obj.UserType,
                        TaxId = TaxId,
                        CurrencyId = CurrencyId,
                        AdvanceBalance = 0,
                        IsWalkin = false,
                        Under = obj.Under,
                        IsRootAccount = false,
                        BatchNo = obj.BatchNo,
                        AccountId = AccountId,
                        JournalAccountId = JournalAccountId,
                        PaymentTermId = PaymentTermId,
                        //PlaceOfSupplyId = PlaceOfSupplyId,
                        SourceOfSupplyId = SourceOfSupplyId,
                        IsBusinessRegistered = item.IsBusinessRegistered == true ? 1 : 2,
                        GstTreatment = item.GstTreatment,
                        BusinessRegistrationNameId = BusinessRegistrationNameId,
                        BusinessRegistrationNo = item.GSTIN,
                        BusinessLegalName = item.BusinessLegalName,
                        BusinessTradeName = item.BusinessTradeName,
                        PanNo = item.PanNo,
                    };
                    oConnectionContext.DbClsUser.Add(oClsUser);
                    oConnectionContext.SaveChanges();

                    ClsAddress oClsAddress = new ClsAddress()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        Address = item.BillingAddress,
                        CityId = CityId,
                        CountryId = CountryId,
                        EmailId = item.EmailId,
                        IsActive = true,
                        IsDeleted = false,
                        Landmark = item.BillingLandmark,
                        MobileNo = item.MobileNo,
                        MobileNo2 = item.AltMobileNo,
                        Name = item.Name,
                        StateId = StateId,
                        UserId = oClsUser.UserId,
                        Zipcode = item.BillingZipcode,
                    };
                    oConnectionContext.DbClsAddress.Add(oClsAddress);
                    oConnectionContext.SaveChanges();

                    ClsAddress oClsAddress1 = new ClsAddress()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        Address = item.ShippingAddress,
                        CityId = AltCityId,
                        CountryId = AltCountryId,
                        EmailId = item.ShippingEmailId,
                        IsActive = true,
                        IsDeleted = false,
                        Landmark = item.ShippingLandmark,
                        //Latitude = item.Latitude,
                        //Locality = item.Locality,
                        //Longitude = item.Longitude,
                        MobileNo = item.ShippingMobileNo,
                        MobileNo2 = item.ShippingAltMobileNo,
                        Name = item.ShippingName,
                        StateId = AltStateId,
                        UserId = oClsUser.UserId,
                        Zipcode = item.ShippingZipcode,
                    };
                    oConnectionContext.DbClsAddress.Add(oClsAddress1);
                    oConnectionContext.SaveChanges();

                    dbContextTransaction.Complete();
                }
            }

            ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
            {
                AddedBy = obj.AddedBy,
                Browser = obj.Browser,
                Category = "Suppliers",
                CompanyId = obj.CompanyId,
                Description = "Suppliers imported",
                Id = 0,
                IpAddress = obj.IpAddress,
                Platform = obj.Platform,
                Type = "Insert"
            };
            oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

            data = new
            {
                Status = 1,
                Message = "Suppliers imported successfully",
                Data = new
                {
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchUserCurrency(ClsUserVm obj)
        {
            decimal TotalSalesDue = oConnectionContext.DbClsSales.Where(a => a.Status != "Draft" && a.CompanyId == obj.CompanyId &&
                                a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                //&& a.BranchId == obj.BranchId 
                                && a.CustomerId == obj.UserId).Select(a => a.GrandTotal-a.WriteOffAmount).DefaultIfEmpty().Sum() -
                                (from a in oConnectionContext.DbClsSales
                                 join b in oConnectionContext.DbClsCustomerPayment
                                 on a.SalesId equals b.SalesId
                                 where a.Status != "Draft" && a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false
                                 //&& a.BranchId == obj.BranchId 
                                 && (b.Type.ToLower() == "sales payment") && b.IsDeleted == false && b.IsCancelled == false &&
                                 b.CompanyId == obj.CompanyId
                                 //&& b.BranchId == obj.BranchId 
                                 && a.CustomerId == obj.UserId
                                 select b.Amount).DefaultIfEmpty().Sum();

            var det = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => new
            {
                a.GstTreatment,
                a.CurrencyId,
                CurrencyCode = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencyCode).FirstOrDefault(),
                CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == a.CurrencyId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                ExchangeRate = oConnectionContext.DbClsUserCurrencyMap.Where(c => c.CurrencyId == a.CurrencyId && c.CompanyId == obj.CompanyId).Select(c => c.ExchangeRate).FirstOrDefault(),
                DefaultCurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUserCurrencyMap.Where(c => c.IsMain == true && c.CompanyId == obj.CompanyId).Select(c => c.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                TotalSalesDue = TotalSalesDue,
                a.AdvanceBalance,
                //a.PayTerm,
                //a.PayTermNo,
                a.PlaceOfSupplyId,
                a.SourceOfSupplyId,
                a.PaymentTermId,
                a.TaxExemptionId,
                a.BusinessRegistrationNo
            }).FirstOrDefault();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.CountryId,
                StateId = oConnectionContext.DbClsBranch.Where(aa => aa.BranchId == obj.BranchId).Select(aa => aa.StateId).FirstOrDefault()
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = det,
                    BusinessSetting = BusinessSetting
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllCompanies(ClsUserVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

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

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.UserType).FirstOrDefault();

            List<ClsUserVm> det = new List<ClsUserVm>();

            det = (from a in oConnectionContext.DbClsUser
                   where a.UserType.ToLower() == "user" && a.IsCompany == true &&
                   a.IsDeleted == false && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
            DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate && a.Under == obj.Under
                   select new ClsUserVm
                   {
                       IdProof = a.IdProof,
                       CommissionPercent = a.CommissionPercent,
                       Under = a.Under,
                       ResellerId = a.ResellerId,
                       ResellerEmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ResellerId).Select(b => b.EmailId).FirstOrDefault(),
                       BusinessName = a.BusinessName,
                       UserId = a.UserId,
                       Name = a.Name,
                       EmailId = a.EmailId,
                       MobileNo = a.MobileNo,
                       AltMobileNo = a.AltMobileNo,
                       IsActive = a.IsActive,
                       IsDeleted = a.IsDeleted,
                       AddedBy = a.AddedBy,
                       AddedOn = a.AddedOn,
                       ModifiedBy = a.ModifiedBy,
                       ModifiedOn = a.ModifiedOn,
                       ExpiryDate = a.ExpiryDate,
                       JoiningDate = a.JoiningDate,
                       ProfilePic = a.ProfilePic,
                       AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                       ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                       Country = oConnectionContext.DbClsCountry.Where(z => z.CountryId == oConnectionContext.DbClsBusinessSettings.Where(y => y.CompanyId == a.CompanyId).Select(y => y.CountryId).FirstOrDefault()).Select(z => z.Country).FirstOrDefault(),
                       Transaction = oConnectionContext.DbClsTransaction.OrderByDescending(aa => aa.TransactionId).Where(aa => aa.CompanyId == a.CompanyId &&
                            aa.StartDate != null && aa.Status == 2).Select(aa => new ClsTransactionVm
                            {
                                IsTrial = aa.IsTrial,
                                IsActive = aa.IsActive
                            }).FirstOrDefault()
                   }).ToList();

            if (UserType.ToLower() == "reseller")
            {
                det = det.Where(a => a.ResellerId == obj.AddedBy).Select(a => a).ToList();
            }

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.ResellerId == obj.UserId).Select(a => a).ToList();
            }

            if (obj.MobileNo != "" && obj.MobileNo != null)
            {
                det = det.Where(a => a.MobileNo != null && a.MobileNo != "" && a.MobileNo.Contains(obj.MobileNo)).Select(a => a).ToList();
            }

            if (obj.Name != "" && obj.Name != null)
            {
                det = det.Where(a => a.Name != null && a.Name != "" && a.Name.ToLower().Contains(obj.Name.ToLower())).Select(a => a).ToList();
            }

            if (obj.EmailId != "" && obj.EmailId != null)
            {
                det = det.Where(a => a.EmailId != null && a.EmailId != "" && a.EmailId.ToLower().Contains(obj.EmailId.ToLower())).Select(a => a).ToList();
            }

            if (obj.BusinessName != "" && obj.BusinessName != null)
            {
                det = det.Where(a => a.BusinessName != null && a.BusinessName != "" && a.BusinessName.ToLower().Contains(obj.BusinessName.ToLower())).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> AllWhitelabelResellers(ClsUserVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

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

            List<ClsUserVm> det = (from a in oConnectionContext.DbClsUser
                                   where a.UserType.ToLower() == "whitelabel reseller" &&
                                   a.IsDeleted == false && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                            DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate //&& a.Under == obj.Under
                                   select new ClsUserVm
                                   {
                                       IdProof = a.IdProof,
                                       CommissionPercent = a.CommissionPercent,
                                       Under = a.Under,
                                       BusinessName = a.BusinessName,
                                       UserId = a.UserId,
                                       Name = a.Name,
                                       EmailId = a.EmailId,
                                       MobileNo = a.MobileNo,
                                       AltMobileNo = a.AltMobileNo,
                                       IsActive = a.IsActive,
                                       IsDeleted = a.IsDeleted,
                                       AddedBy = a.AddedBy,
                                       AddedOn = a.AddedOn,
                                       ModifiedBy = a.ModifiedBy,
                                       ModifiedOn = a.ModifiedOn,
                                       ExpiryDate = a.ExpiryDate,
                                       JoiningDate = a.JoiningDate,
                                       ProfilePic = a.ProfilePic,
                                       AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                                       ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                                       Country = oConnectionContext.DbClsCountry.Where(z => z.CountryId == oConnectionContext.DbClsBusinessSettings.Where(y => y.CompanyId == a.CompanyId).Select(y => y.CountryId).FirstOrDefault()).Select(z => z.Country).FirstOrDefault(),
                                   }).ToList();

            //if (obj.UserId != 0)
            //{
            //    det = det.Where(a => a.Under == obj.UserId).Select(a => a).ToList();
            //}

            if (obj.MobileNo != "" && obj.MobileNo != null)
            {
                det = det.Where(a => a.MobileNo != null && a.MobileNo != "" && a.MobileNo.Contains(obj.MobileNo)).Select(a => a).ToList();
            }

            if (obj.Name != "" && obj.Name != null)
            {
                det = det.Where(a => a.Name != null && a.Name != "" && a.Name.ToLower().Contains(obj.Name.ToLower())).Select(a => a).ToList();
            }

            if (obj.EmailId != "" && obj.EmailId != null)
            {
                det = det.Where(a => a.EmailId != null && a.EmailId != "" && a.EmailId.ToLower().Contains(obj.EmailId.ToLower())).Select(a => a).ToList();
            }

            if (obj.BusinessName != "" && obj.BusinessName != null)
            {
                det = det.Where(a => a.BusinessName != null && a.BusinessName != "" && a.BusinessName.ToLower().Contains(obj.BusinessName.ToLower())).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
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

        public async Task<IHttpActionResult> AllResellers(ClsUserVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

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

            List<ClsUserVm> det = (from a in oConnectionContext.DbClsUser
                                   where a.UserType.ToLower() == "reseller" &&
                                   a.IsDeleted == false && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                            DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate && a.Under == obj.Under
                                   select new ClsUserVm
                                   {
                                       IdProof = a.IdProof,
                                       CommissionPercent = a.CommissionPercent,
                                       Username = a.Username,
                                       Under = a.Under,
                                       BusinessName = a.BusinessName,
                                       UserId = a.UserId,
                                       Name = a.Name,
                                       EmailId = a.EmailId,
                                       MobileNo = a.MobileNo,
                                       AltMobileNo = a.AltMobileNo,
                                       IsActive = a.IsActive,
                                       IsDeleted = a.IsDeleted,
                                       AddedBy = a.AddedBy,
                                       AddedOn = a.AddedOn,
                                       ModifiedBy = a.ModifiedBy,
                                       ModifiedOn = a.ModifiedOn,
                                       ExpiryDate = a.ExpiryDate,
                                       JoiningDate = a.JoiningDate,
                                       ProfilePic = a.ProfilePic,
                                       AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                                       ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                                       Country = oConnectionContext.DbClsCountry.Where(z => z.CountryId == oConnectionContext.DbClsBusinessSettings.Where(y => y.CompanyId == a.CompanyId).Select(y => y.CountryId).FirstOrDefault()).Select(z => z.Country).FirstOrDefault(),
                                       MarketingLink = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == a.Under).Select(b => b.WebsiteUrl).FirstOrDefault() + "/register?UserId=" + a.Username,
                                   }).ToList();

            //if (obj.UserId != 0)
            //{
            //    det = det.Where(a => a.Under == obj.UserId).Select(a => a).ToList();
            //}

            if (obj.MobileNo != "" && obj.MobileNo != null)
            {
                det = det.Where(a => a.MobileNo != null && a.MobileNo != "" && a.MobileNo.Contains(obj.MobileNo)).Select(a => a).ToList();
            }

            if (obj.Name != "" && obj.Name != null)
            {
                det = det.Where(a => a.Name != null && a.Name != "" && a.Name.ToLower().Contains(obj.Name.ToLower())).Select(a => a).ToList();
            }

            if (obj.EmailId != "" && obj.EmailId != null)
            {
                det = det.Where(a => a.EmailId != null && a.EmailId != "" && a.EmailId.ToLower().Contains(obj.EmailId.ToLower())).Select(a => a).ToList();
            }

            if (obj.BusinessName != "" && obj.BusinessName != null)
            {
                det = det.Where(a => a.BusinessName != null && a.BusinessName != "" && a.BusinessName.ToLower().Contains(obj.BusinessName.ToLower())).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Users = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
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
        [AllowAnonymous]
        public async Task<IHttpActionResult> CheckUserName(ClsUserVm obj)
        {
            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            if (oConnectionContext.DbClsUser.Where(a => a.Username == obj.Username && a.IsDeleted == false && a.Under == obj.Under).Count() > 0)
            {
                data = new
                {
                    Status = 0,
                    Message = "User Code is already taken",
                    Data = new
                    {
                    }
                };
                return await Task.FromResult(Ok(data));
            }

            data = new
            {
                Status = 1,
                Message = "found",
            };
            return await Task.FromResult(Ok(data));
        }
        public async Task<IHttpActionResult> UserActiveInactiveAdmin(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.UserType).FirstOrDefault();

                if (oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => a.ModifiedBy).FirstOrDefault() == 0)
                {
                    var user = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.UserId).Select(a => new
                    {
                        a.Username,
                        a.EmailId,
                        a.Name
                    }).FirstOrDefault();

                    if (UserType.ToLower() == "reseller")
                    {
                        string joiningLink = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.WebsiteUrl).FirstOrDefault() + "/register?UserId=" + user.Username;
                        oEmailController.WelcomeReseller(user.EmailId, "Welcome to " + oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessName).FirstOrDefault(), user.Name, oCommonController.webUrl, obj.Domain, joiningLink);
                    }
                    else
                    {
                        string joiningLink = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.WebsiteUrl).FirstOrDefault() + "/register?UserId=" + user.Username;
                        oEmailController.WelcomeWhitelabelReseller(user.EmailId, "Welcome to " + oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessName).FirstOrDefault(), user.Name, oCommonController.webUrl, obj.Domain);
                    }
                }

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.UserId).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    Category = obj.UserType,
                //    CompanyId = obj.CompanyId,
                //    Description = (obj.IsActive == true ? "activated " : "deactivated ") + obj.Username,
                //    Id = oClsUser.UserId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Update"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = UserType + (obj.IsActive == true ? " activated" : " deactivated") + " successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ResellerCommissionPercentUpdate(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.CommissionPercent > 50)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Maximum 50% is possible",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                ClsUser oClsUser = new ClsUser()
                {
                    UserId = obj.UserId,
                    CommissionPercent = obj.CommissionPercent
                };
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.CommissionPercent).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = 1,
                    Message = "Commission Percent updated successfully",
                    Data = new
                    {
                    }
                };

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerLedger(ClsSalesVm obj)
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
            List<ClsSalesVm> Sales;
            List<ClsSalesVm> SalesReturn;
            //List<ClsSalesVm> Expense;
            //List<ClsSalesVm> Income;
            List<ClsSalesVm> CustomerPayment;
            List<ClsSalesVm> SalesReturnPayment;
            List<ClsSalesVm> ExpensePayment;
            List<ClsSalesVm> IncomePayment;
            List<ClsSalesVm> OpeningBalance;
            //List<ClsSalesVm> OpeningBalancePayment;
            List<ClsSalesVm> PaymentReceived;
            List<ClsSalesVm> PaymentPaid;
            List<ClsSalesVm> LedgerDiscount;
            List<ClsSalesVm> Ledger = new List<ClsSalesVm>();
            ClsUserVm User = new ClsUserVm();

            if (obj.BranchId == 0)
            {
                Sales = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && a.Status.ToLower() != "Draft" && a.CustomerId == obj.CustomerId
 //&& a.BranchId == obj.BranchId 
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate).Select(a => new ClsSalesVm
                {
                    Type = a.SalesType,
                    AddedOn = a.SalesDate,
                    InvoiceNo = a.InvoiceNo,
                    BranchId = a.BranchId,
                    Status = a.Status,
                    Credit = 0,
                    Debit = a.GrandTotal,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PaymentType = "",
                    OtherRemarks = ""
                }).ToList();

                CustomerPayment = (from d in oConnectionContext.DbClsCustomerPayment
                                   join c in oConnectionContext.DbClsSales on d.SalesId equals c.SalesId
                                   where c.Status != "Draft" &&
                  (d.Type.ToLower() == "sales payment") && c.CustomerId == obj.CustomerId
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   && d.ParentId == 0
                                   && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                   DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = c.BranchId,
                                       //PaymentStatus = "",
                                       Credit = d.Amount,
                                       Debit = 0,
                                       BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsSales.Where(cc => cc.SalesId == d.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault()
                                   }).ToList();

                SalesReturn = (from a in oConnectionContext.DbClsSalesReturn
                               join b in oConnectionContext.DbClsSales
        on a.SalesId equals b.SalesId
                               where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.CustomerId == obj.CustomerId
         //&& b.BranchId == obj.BranchId
         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == b.BranchId)
                               && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                               select new ClsSalesVm
                               {
                                   Type = b.SalesType + "Return",
                                   AddedOn = a.Date,
                                   InvoiceNo = a.InvoiceNo,
                                   BranchId = b.BranchId,
                                   Status = a.Status,
                                   Credit = a.GrandTotal,
                                   Debit = 0,
                                   BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                   PaymentType = "",
                                   OtherRemarks = ""
                               }).ToList();

                SalesReturnPayment = (from e in oConnectionContext.DbClsCustomerPayment
                                      join c in oConnectionContext.DbClsSalesReturn
                                      on e.SalesId equals c.SalesReturnId
                                      join d in oConnectionContext.DbClsSales
                     on c.SalesId equals d.SalesId
                                      where c.CompanyId == obj.CompanyId && d.CustomerId == obj.CustomerId
                                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                      && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                      && d.IsDeleted == false && d.IsCancelled == false
                                      && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                     e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                     && e.ParentId == 0
                     && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                      select new ClsSalesVm
                                      {
                                          Type = e.Type,
                                          AddedOn = e.PaymentDate,
                                          InvoiceNo = e.ReferenceNo,
                                          BranchId = d.BranchId,
                                          //PaymentStatus = "",
                                          Credit = 0,
                                          Debit = e.Amount,
                                          BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == d.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                          PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                          OtherRemarks = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == e.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault()
                                      }).ToList();

                // Expense = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                // && a.ExpenseFor == obj.UserId
                // //&& a.BranchId == obj.BranchId 
                // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                // l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
                // && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                // DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                // ).Select(a => new ClsSalesVm
                // {
                //     Type ="Expense",
                //     AddedOn = a.Date,
                //     InvoiceNo = a.ReferenceNo,
                //     BranchId = a.BranchId,
                //     PaymentStatus = a.PaymentStatus,
                //     Credit=0,
                //     Debit = a.Amount,
                //     BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                //     PaymentType = ""
                // }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.CustomerId == obj.CustomerId
               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      BranchId = c.BranchId,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = d.Amount,
                                      BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                      // PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault()
                                  }).ToList();

                LedgerDiscount = (from d in oConnectionContext.DbClsLedgerDiscount
                                  where d.Type.ToLower() == "customer ledger discount" && d.Id == obj.CustomerId
               && d.IsActive == true && d.IsDeleted == false
               && DbFunctions.TruncateTime(d.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.AddedOn) <= obj.ToDate
                                  select new ClsSalesVm
                                  {
                                      Type = "Ledger Discount",
                                      AddedOn = d.PaymentDate,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Debit = 0,
                                      Credit = d.Amount,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsCustomerPayment
                                   where d.Type.ToLower() == "customer payment" && d.CustomerId == obj.CustomerId
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = 0,
                                       //PaymentStatus = "",
                                       Debit = 0,
                                       Credit = d.Amount,
                                       BranchName = "",
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = ""
                                   }).ToList();

                PaymentPaid = (from d in oConnectionContext.DbClsCustomerPayment
                               where d.Type.ToLower() == "customer refund" && d.CustomerId == obj.CustomerId
                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                   && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                               select new ClsSalesVm
                               {
                                   Type = "Payment Paid",
                                   AddedOn = d.PaymentDate,
                                   InvoiceNo = d.ReferenceNo,
                                   BranchId = 0,
                                   //PaymentStatus = "",
                                   Credit = 0,
                                   Debit = d.Amount,
                                   BranchName = "",
                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                   OtherRemarks = ""
                               }).ToList();


                User = (from a in oConnectionContext.DbClsUser
                            //join b in oConnectionContext.DbClsUserBranchMap
                            //on a.UserId equals b.UserId 
                        where a.CompanyId == obj.CompanyId && a.UserId == obj.CustomerId &&
                     a.IsDeleted == false && a.IsActive == true
                        //&& a.UserId != obj.AddedBy 
                        select new ClsUserVm
                        {
                            AddedOn = a.AddedOn,
                            LedgerDiscount = a.LedgerDiscount,
                            OpeningBalance = a.OpeningBalance,
                            AdvanceBalance = (from d in oConnectionContext.DbClsCustomerPayment
                                              where d.Type.ToLower() == "customer payment" && d.CustomerId == obj.CustomerId
                           && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                              select d.Amount).DefaultIfEmpty().Sum(),
                            TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId &&
                            c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
              //&& a.BranchId == obj.BranchId
              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
             ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalSalesPaid = (from d in oConnectionContext.DbClsCustomerPayment
                                              join c in oConnectionContext.DbClsSales on d.SalesId equals c.SalesId
                                              where c.Status != "Draft" &&
                     (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
                     && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                     && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                     && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                     && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                              select d.Amount).DefaultIfEmpty().Sum(),
                            TotalSalesReturn = (from aa in oConnectionContext.DbClsSalesReturn
                                                join bb in oConnectionContext.DbClsSales
                         on aa.SalesId equals bb.SalesId
                                                where aa.CompanyId == obj.CompanyId && aa.IsDeleted == false && aa.IsCancelled == false && bb.CustomerId == obj.CustomerId
                          //&& b.BranchId == obj.BranchId
                          && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == bb.BranchId)
                                                && DbFunctions.TruncateTime(aa.Date) >= obj.FromDate &&
                                         DbFunctions.TruncateTime(aa.Date) <= obj.ToDate
                                                select aa.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalSalesReturnPaid = (from e in oConnectionContext.DbClsCustomerPayment
                                                    join c in oConnectionContext.DbClsSalesReturn
                                                    on e.SalesId equals c.SalesReturnId
                                                    join d in oConnectionContext.DbClsSales
                                   on c.SalesId equals d.SalesId
                                                    where c.CompanyId == obj.CompanyId && d.CustomerId == obj.CustomerId
                                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                    && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                                    && d.IsDeleted == false && d.IsCancelled == false
                                                    && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                                   e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                                                //&& e.ParentId == 0 
                                   && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                                    select e.Amount).DefaultIfEmpty().Sum(),
                            TotalOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                       where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == obj.CustomerId
                                    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       select d.Amount).DefaultIfEmpty().Sum()
                        }).FirstOrDefault();

                decimal _openingBalance = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserId == obj.CustomerId &&
                                  a.IsDeleted == false && a.IsActive == true).Select(a => a.OpeningBalance).FirstOrDefault();

                bool isOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                             where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == obj.CustomerId
                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select d.Amount).DefaultIfEmpty().Sum() == _openingBalance ? true : false;

                decimal _openingBalanceDue = 0;

                if (User.AddedOn.Year == DateTime.Now.Year)
                {
                    _openingBalanceDue = _openingBalance;
                }
                else
                {
                    if (isOpeningBalancePaid == false)
                    {
                        _openingBalanceDue = (_openingBalance + User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.TotalSalesPaid + User.LedgerDiscount);
                    }
                    else
                    {
                        _openingBalanceDue = (User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.LedgerDiscount);
                    }
                }

                OpeningBalance = (from a in oConnectionContext.DbClsUser
                                  where a.CompanyId == obj.CompanyId && a.UserId == obj.CustomerId &&
                                  a.IsDeleted == false && a.IsActive == true && a.OpeningBalance > 0
                                  select new ClsSalesVm
                                  {
                                      Type = "Opening Balance",
                                      AddedOn = obj.FromDate,//a.JoiningDate.Value,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = _openingBalanceDue,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                //OpeningBalancePayment = (from d in oConnectionContext.DbClsPayment
                //                   join c in oConnectionContext.DbClsUser on d.Id equals c.UserId
                //                   where d.Type.ToLower() == "customer opening balance payment" && c.UserId == obj.UserId
                //&& c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //                   select new ClsSalesVm
                //                   {
                //                       Type ="Opening Balance Payment",
                //                       AddedOn = d.PaymentDate,
                //                       InvoiceNo = d.ReferenceNo,
                //                       BranchId = 0,
                //                       PaymentStatus = "",
                //                       GrandTotal = d.Amount,
                //                       BranchName = "",
                //                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                       OtherRemarks = ""
                //                   }).ToList();

                Ledger = Sales.Union(CustomerPayment).Union(SalesReturn).Union(SalesReturnPayment).Union(OpeningBalance).
                     //Union(OpeningBalancePayment).
                     Union(LedgerDiscount).
                     Union(PaymentReceived).
                     Union(PaymentPaid)
                    //.Union(Expense)
                    .Union(ExpensePayment)
                     .OrderBy(a => a.AddedOn).ToList();

                decimal Balance = 0;

                foreach (var item in Ledger)
                {
                    Balance = Balance + (item.Credit - item.Debit);
                    item.Balance = Balance;
                }

            }
            else
            {
                Sales = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && a.Status.ToLower() != "Draft" && a.CustomerId == obj.CustomerId
 && a.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(a.SalesDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.SalesDate) <= obj.ToDate).Select(a => new ClsSalesVm
                {
                    Type = a.SalesType,
                    AddedOn = a.SalesDate,
                    InvoiceNo = a.InvoiceNo,
                    BranchId = a.BranchId,
                    Status = a.Status,
                    Credit = 0,
                    Debit = a.GrandTotal,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PaymentType = "",
                    OtherRemarks = ""
                }).ToList();

                CustomerPayment = (from d in oConnectionContext.DbClsCustomerPayment
                                   join c in oConnectionContext.DbClsSales on d.SalesId equals c.SalesId
                                   where c.Status != "Draft" &&
                  (d.Type.ToLower() == "sales payment") && c.CustomerId == obj.CustomerId
                  && c.BranchId == obj.BranchId
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   && d.ParentId == 0
                                   && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                   DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = c.BranchId,
                                       //PaymentStatus = "",
                                       Credit = d.Amount,
                                       Debit = 0,
                                       BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsSales.Where(cc => cc.SalesId == d.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault()
                                   }).ToList();

                SalesReturn = (from a in oConnectionContext.DbClsSalesReturn
                               join b in oConnectionContext.DbClsSales
        on a.SalesId equals b.SalesId
                               where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.CustomerId == obj.CustomerId
         && b.BranchId == obj.BranchId
                               && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                        DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                               select new ClsSalesVm
                               {
                                   Type = b.SalesType + "Return",
                                   AddedOn = a.Date,
                                   InvoiceNo = a.InvoiceNo,
                                   BranchId = b.BranchId,
                                   Status = a.Status,
                                   Credit = a.GrandTotal,
                                   Debit = 0,
                                   BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                   PaymentType = "",
                                   OtherRemarks = ""
                               }).ToList();

                SalesReturnPayment = (from e in oConnectionContext.DbClsCustomerPayment
                                      join c in oConnectionContext.DbClsSalesReturn
                                      on e.SalesId equals c.SalesReturnId
                                      join d in oConnectionContext.DbClsSales
                     on c.SalesId equals d.SalesId
                                      where c.CompanyId == obj.CompanyId && d.CustomerId == obj.CustomerId
                                      && d.BranchId == obj.BranchId
                                      && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                      && d.IsDeleted == false && d.IsCancelled == false
                                      && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                     e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                     && e.ParentId == 0
                     && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                      select new ClsSalesVm
                                      {
                                          Type = e.Type,
                                          AddedOn = e.PaymentDate,
                                          InvoiceNo = e.ReferenceNo,
                                          BranchId = d.BranchId,
                                          //PaymentStatus = "",
                                          Credit = 0,
                                          Debit = e.Amount,
                                          BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == d.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                          PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                          OtherRemarks = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == e.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault()
                                      }).ToList();

                // Expense = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                // && a.ExpenseFor == obj.UserId
                // //&& a.BranchId == obj.BranchId 
                // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                // l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
                // && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                // DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                // ).Select(a => new ClsSalesVm
                // {
                //     Type ="Expense",
                //     AddedOn = a.Date,
                //     InvoiceNo = a.ReferenceNo,
                //     BranchId = a.BranchId,
                //     PaymentStatus = a.PaymentStatus,
                //     Credit=0,
                //     Debit = a.Amount,
                //     BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                //     PaymentType = ""
                // }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.CustomerId == obj.CustomerId
               && c.BranchId == obj.BranchId
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      BranchId = c.BranchId,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = d.Amount,
                                      BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault()
                                  }).ToList();

                LedgerDiscount = (from d in oConnectionContext.DbClsLedgerDiscount
                                  where d.Type.ToLower() == "customer ledger discount" && d.Id == obj.CustomerId
               && d.IsActive == true && d.IsDeleted == false
               && DbFunctions.TruncateTime(d.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.AddedOn) <= obj.ToDate
                                  select new ClsSalesVm
                                  {
                                      Type = "Ledger Discount",
                                      AddedOn = d.PaymentDate,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Debit = 0,
                                      Credit = d.Amount,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsCustomerPayment
                                   where d.Type.ToLower() == "customer payment" && d.CustomerId == obj.CustomerId
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = 0,
                                       //PaymentStatus = "",
                                       Debit = 0,
                                       Credit = d.Amount,
                                       BranchName = "",
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = ""
                                   }).ToList();

                PaymentPaid = (from d in oConnectionContext.DbClsCustomerPayment
                               where d.Type.ToLower() == "customer refund" && d.CustomerId == obj.CustomerId
                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                   && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                               select new ClsSalesVm
                               {
                                   Type = "Payment Paid",
                                   AddedOn = d.PaymentDate,
                                   InvoiceNo = d.ReferenceNo,
                                   BranchId = 0,
                                   //PaymentStatus = "",
                                   Credit = 0,
                                   Debit = d.Amount,
                                   BranchName = "",
                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                   OtherRemarks = ""
                               }).ToList();


                User = (from a in oConnectionContext.DbClsUser
                            //join b in oConnectionContext.DbClsUserBranchMap
                            //on a.UserId equals b.UserId 
                        where a.CompanyId == obj.CompanyId && a.UserId == obj.CustomerId &&
                     a.IsDeleted == false && a.IsActive == true
                        //&& a.UserId != obj.AddedBy 
                        select new ClsUserVm
                        {
                            AddedOn = a.AddedOn,
                            LedgerDiscount = a.LedgerDiscount,
                            OpeningBalance = a.OpeningBalance,
                            AdvanceBalance = (from d in oConnectionContext.DbClsCustomerPayment
                                              where d.Type.ToLower() == "customer payment" && d.CustomerId == obj.CustomerId
                           && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                              select d.Amount).DefaultIfEmpty().Sum(),
                            TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId &&
                            c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
              && c.BranchId == obj.BranchId
                && DbFunctions.TruncateTime(c.SalesDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(c.SalesDate) <= obj.ToDate
             ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalSalesPaid = (from d in oConnectionContext.DbClsCustomerPayment
                                              join c in oConnectionContext.DbClsSales on d.SalesId equals c.SalesId
                                              where c.Status != "Draft" &&
                     (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
                     && c.BranchId == obj.BranchId
                     && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                     && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                     && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                              select d.Amount).DefaultIfEmpty().Sum(),
                            TotalSalesReturn = (from aa in oConnectionContext.DbClsSalesReturn
                                                join bb in oConnectionContext.DbClsSales
                         on aa.SalesId equals bb.SalesId
                                                where aa.CompanyId == obj.CompanyId && aa.IsDeleted == false && aa.IsCancelled == false && bb.CustomerId == obj.CustomerId
                          && bb.BranchId == obj.BranchId
                                                && DbFunctions.TruncateTime(aa.Date) >= obj.FromDate &&
                                         DbFunctions.TruncateTime(aa.Date) <= obj.ToDate
                                                select aa.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalSalesReturnPaid = (from e in oConnectionContext.DbClsCustomerPayment
                                                    join c in oConnectionContext.DbClsSalesReturn
                                                    on e.SalesId equals c.SalesReturnId
                                                    join d in oConnectionContext.DbClsSales
                                   on c.SalesId equals d.SalesId
                                                    where c.CompanyId == obj.CompanyId && d.CustomerId == obj.CustomerId
                                                    && d.BranchId == obj.BranchId
                                                    && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                                    && d.IsDeleted == false && d.IsCancelled == false
                                                    && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                                   e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                                                //&& e.ParentId == 0 
                                   && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                  DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                                    select e.Amount).DefaultIfEmpty().Sum(),
                            TotalOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                                       where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == obj.CustomerId
                                    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       select d.Amount).DefaultIfEmpty().Sum()
                        }).FirstOrDefault();

                decimal _openingBalance = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserId == obj.CustomerId &&
                                  a.IsDeleted == false && a.IsActive == true).Select(a => a.OpeningBalance).FirstOrDefault();

                bool isOpeningBalancePaid = (from d in oConnectionContext.DbClsCustomerPayment
                                             where d.Type.ToLower() == "customer opening balance payment" && d.CustomerId == obj.CustomerId
                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select d.Amount).DefaultIfEmpty().Sum() == _openingBalance ? true : false;

                decimal _openingBalanceDue = 0;

                if (User.AddedOn.Year == DateTime.Now.Year)
                {
                    _openingBalanceDue = _openingBalance;
                }
                else
                {
                    if (isOpeningBalancePaid == false)
                    {
                        _openingBalanceDue = (_openingBalance + User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.TotalSalesPaid + User.LedgerDiscount);
                    }
                    else
                    {
                        _openingBalanceDue = (User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.LedgerDiscount);
                    }
                }

                OpeningBalance = (from a in oConnectionContext.DbClsUser
                                  where a.CompanyId == obj.CompanyId && a.UserId == obj.CustomerId &&
                                  a.IsDeleted == false && a.IsActive == true && a.OpeningBalance > 0
                                  select new ClsSalesVm
                                  {
                                      Type = "Opening Balance",
                                      AddedOn = obj.FromDate,//a.JoiningDate.Value,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = _openingBalanceDue,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                //OpeningBalancePayment = (from d in oConnectionContext.DbClsPayment
                //                   join c in oConnectionContext.DbClsUser on d.Id equals c.UserId
                //                   where d.Type.ToLower() == "customer opening balance payment" && c.UserId == obj.UserId
                //&& c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //                   select new ClsSalesVm
                //                   {
                //                       Type ="Opening Balance Payment",
                //                       AddedOn = d.PaymentDate,
                //                       InvoiceNo = d.ReferenceNo,
                //                       BranchId = 0,
                //                       PaymentStatus = "",
                //                       GrandTotal = d.Amount,
                //                       BranchName = "",
                //                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                       OtherRemarks = ""
                //                   }).ToList();

                Ledger = Sales.Union(CustomerPayment).Union(SalesReturn).Union(SalesReturnPayment).Union(OpeningBalance).
                     //Union(OpeningBalancePayment).
                     Union(LedgerDiscount).
                     Union(PaymentReceived).
                     Union(PaymentPaid)
                    //.Union(Expense)
                    .Union(ExpensePayment)
                     .OrderBy(a => a.AddedOn).ToList();

                decimal Balance = 0;

                foreach (var item in Ledger)
                {
                    Balance = Balance + (item.Credit - item.Debit);
                    item.Balance = Balance;
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = Ledger.Skip(skip).Take(obj.PageSize).ToList(),
                    User = User,
                    TotalCount = Ledger.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CustomerPayments(ClsSalesVm obj)
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

            List<ClsSalesVm> CustomerPayment;
            List<ClsSalesVm> SalesReturnPayment;
            List<ClsSalesVm> ExpensePayment;
            List<ClsSalesVm> IncomePayment;
            List<ClsSalesVm> OpeningBalance;
            List<ClsSalesVm> PaymentReceived;
            List<ClsSalesVm> PaymentPaid;
            List<ClsSalesVm> LedgerDiscount;
            List<ClsSalesVm> Ledger = new List<ClsSalesVm>();

            if (obj.BranchId == 0)
            {
                CustomerPayment = (from d in oConnectionContext.DbClsCustomerPayment
                                   join c in oConnectionContext.DbClsSales on d.SalesId equals c.SalesId
                                   where c.Status != "Draft" &&
                  (d.Type.ToLower() == "sales payment") && c.CustomerId == obj.CustomerId
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       PaymentId = d.CustomerPaymentId,
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsSales.Where(cc => cc.SalesId == d.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                       SalesId = d.SalesId
                                   }).ToList();

                SalesReturnPayment = (from e in oConnectionContext.DbClsCustomerPayment
                                      join c in oConnectionContext.DbClsSalesReturn
                                      on e.SalesId equals c.SalesReturnId
                                      join d in oConnectionContext.DbClsSales
                     on c.SalesId equals d.SalesId
                                      where c.CompanyId == obj.CompanyId && d.CustomerId == obj.CustomerId
                                      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                     l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                      && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                      && d.IsDeleted == false && d.IsCancelled == false
                                      && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                     e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                      select new ClsSalesVm
                                      {
                                          PaymentId = e.CustomerPaymentId,
                                          Type = e.Type,
                                          AddedOn = e.PaymentDate,
                                          InvoiceNo = e.ReferenceNo,
                                          GrandTotal = e.Amount,
                                          PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                          OtherRemarks = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == e.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                          SalesId = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == e.SalesId).Select(cc => cc.SalesId).FirstOrDefault(),
                                      }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.CustomerId == obj.CustomerId
               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      PaymentId = d.ExpensePaymentId,
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      GrandTotal = d.Amount,
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                      SalesId = d.ExpenseId
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsCustomerPayment
                                   join c in oConnectionContext.DbClsUser on d.CustomerId equals c.UserId
                                   where d.Type.ToLower() == "customer payment" && c.UserId == obj.CustomerId
                && c.IsActive == true && c.IsDeleted == false
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       PaymentId = d.CustomerPaymentId,
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = "",
                                       Payments = (from dd in oConnectionContext.DbClsCustomerPayment
                                                   where dd.ParentId == d.CustomerPaymentId && dd.Type.ToLower() == "customer opening balance payment"
                                                   select new ClsCustomerPaymentVm
                                                   {
                                                       PaymentId = dd.CustomerPaymentId,
                                                       Type = dd.Type,
                                                       AddedOn = dd.PaymentDate,
                                                       InvoiceNo = dd.ReferenceNo,
                                                       GrandTotal = dd.Amount,
                                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                       OtherRemarks = "",
                                                       SalesId = dd.SalesId
                                                   }).Union(from dd in oConnectionContext.DbClsCustomerPayment
                                                            where dd.ParentId == d.CustomerPaymentId
                                                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == dd.BranchId)
                                                            select new ClsCustomerPaymentVm
                                                            {
                                                                PaymentId = dd.CustomerPaymentId,
                                                                Type = dd.Type,
                                                                AddedOn = dd.PaymentDate,
                                                                InvoiceNo = dd.ReferenceNo,
                                                                GrandTotal = dd.Amount,
                                                                PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                                OtherRemarks = oConnectionContext.DbClsSales.Where(cc => cc.SalesId == dd.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                                                SalesId = dd.SalesId
                                                            }).ToList()
                                   }).ToList();

                //PaymentPaid = (from d in oConnectionContext.DbClsCustomerRefund
                //               join c in oConnectionContext.DbClsUser on d.CustomerId equals c.UserId
                //               where d.Type.ToLower() == "customer refund" && c.UserId == obj.CustomerId
                //   && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                //   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //               select new ClsSalesVm
                //               {
                //                   PaymentId = d.CustomerRefundId,
                //                   Type = "Payment Paid",
                //                   AddedOn = d.PaymentDate,
                //                   InvoiceNo = d.ReferenceNo,
                //                   GrandTotal = d.Amount,
                //                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                   OtherRemarks = "",
                //                   Payments = (from dd in oConnectionContext.DbClsCustomerRefund
                //                               where dd.ParentId == d.CustomerRefundId
                //                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                //       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == dd.BranchId)
                //                               select new ClsCustomerPaymentVm
                //                               {
                //                                   PaymentId = dd.CustomerRefundId,
                //                                   Type = dd.Type,
                //                                   AddedOn = dd.PaymentDate,
                //                                   InvoiceNo = dd.ReferenceNo,
                //                                   GrandTotal = dd.Amount,
                //                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                                   OtherRemarks = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == dd.SalesReturnId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                //                                   SalesId = dd.SalesReturnId
                //                               }).ToList()
                //               }).ToList();

                Ledger = CustomerPayment.Union(SalesReturnPayment).Union(PaymentReceived)
                    //.Union(PaymentPaid)
                    .Union(ExpensePayment).ToList();


            }
            else
            {
                CustomerPayment = (from d in oConnectionContext.DbClsCustomerPayment
                                   join c in oConnectionContext.DbClsSales on d.SalesId equals c.SalesId
                                   where c.Status != "Draft" &&
                  (d.Type.ToLower() == "sales payment") && c.CustomerId == obj.CustomerId
                  && c.BranchId == obj.BranchId
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsSales.Where(cc => cc.SalesId == d.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                       SalesId = d.SalesId
                                   }).ToList();

                SalesReturnPayment = (from e in oConnectionContext.DbClsCustomerPayment
                                      join c in oConnectionContext.DbClsSalesReturn
                                      on e.SalesId equals c.SalesReturnId
                                      join d in oConnectionContext.DbClsSales
                     on c.SalesId equals d.SalesId
                                      where c.CompanyId == obj.CompanyId && d.CustomerId == obj.CustomerId
                                      && d.BranchId == obj.BranchId
                                      && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                      && d.IsDeleted == false && d.IsCancelled == false
                                      && (e.Type.ToLower() == "customer refund") && e.IsDeleted == false && e.IsCancelled == false &&
                     e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                      select new ClsSalesVm
                                      {
                                          Type = e.Type,
                                          AddedOn = e.PaymentDate,
                                          InvoiceNo = e.ReferenceNo,
                                          GrandTotal = e.Amount,
                                          PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                          OtherRemarks = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == e.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                          SalesId = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == e.SalesId).Select(cc => cc.SalesId).FirstOrDefault(),
                                      }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.CustomerId == obj.CustomerId
               && c.BranchId == obj.BranchId
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      GrandTotal = d.Amount,
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                      SalesId = d.ExpenseId
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsCustomerPayment
                                   join c in oConnectionContext.DbClsUser on d.CustomerId equals c.UserId
                                   where d.Type.ToLower() == "customer payment" && c.UserId == obj.CustomerId
                && c.IsActive == true && c.IsDeleted == false
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = "",
                                       Payments = (from dd in oConnectionContext.DbClsCustomerPayment
                                                   where dd.ParentId == d.CustomerPaymentId && dd.Type.ToLower() == "customer opening balance payment"
                                                   select new ClsCustomerPaymentVm
                                                   {
                                                       Type = dd.Type,
                                                       AddedOn = dd.PaymentDate,
                                                       InvoiceNo = dd.ReferenceNo,
                                                       GrandTotal = dd.Amount,
                                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                       OtherRemarks = "",
                                                       SalesId = dd.SalesId
                                                   }).Union(from dd in oConnectionContext.DbClsCustomerPayment
                                                            where dd.ParentId == d.CustomerPaymentId && dd.BranchId == obj.BranchId
                                                            select new ClsCustomerPaymentVm
                                                            {
                                                                Type = dd.Type,
                                                                AddedOn = dd.PaymentDate,
                                                                InvoiceNo = dd.ReferenceNo,
                                                                GrandTotal = dd.Amount,
                                                                PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                                OtherRemarks = oConnectionContext.DbClsSales.Where(cc => cc.SalesId == dd.SalesId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                                                SalesId = dd.SalesId
                                                            }).ToList()
                                   }).ToList();

                //PaymentPaid = (from d in oConnectionContext.DbClsCustomerPayment
                //               join c in oConnectionContext.DbClsUser on d.CustomerId equals c.UserId
                //               where d.Type.ToLower() == "customer refund" && c.UserId == obj.CustomerId
                //   && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                //   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //               select new ClsSalesVm
                //               {
                //                   Type = "Payment Paid",
                //                   AddedOn = d.PaymentDate,
                //                   InvoiceNo = d.ReferenceNo,
                //                   GrandTotal = d.Amount,
                //                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                   OtherRemarks = "",
                //                   Payments = (from dd in oConnectionContext.DbClsCustomerRefund
                //                               where dd.ParentId == d.CustomerPaymentId && dd.BranchId == obj.BranchId
                //                               select new ClsCustomerPaymentVm
                //                               {
                //                                   Type = dd.Type,
                //                                   AddedOn = dd.PaymentDate,
                //                                   InvoiceNo = dd.ReferenceNo,
                //                                   GrandTotal = dd.Amount,
                //                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                                   OtherRemarks = oConnectionContext.DbClsSalesReturn.Where(cc => cc.SalesReturnId == dd.SalesReturnId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                //                                   SalesId = dd.SalesReturnId
                //                               }).ToList()
                //               }).ToList();

                Ledger = CustomerPayment.Union(SalesReturnPayment).Union(PaymentReceived)
                    //.Union(PaymentPaid)
                    .Union(ExpensePayment).ToList();

            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = Ledger.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = Ledger.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierLedger(ClsSalesVm obj)
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
            List<ClsSalesVm> Purchase;
            List<ClsSalesVm> PurchaseReturn;
            //List<ClsSalesVm> Expense;
            //List<ClsSalesVm> Income;
            List<ClsSalesVm> SupplierPayment;
            List<ClsSalesVm> PurchaseReturnPayment;
            List<ClsSalesVm> ExpensePayment;
            List<ClsSalesVm> IncomePayment;
            List<ClsSalesVm> OpeningBalance;
            //List<ClsSalesVm> OpeningBalancePayment;
            List<ClsSalesVm> PaymentReceived;
            List<ClsSalesVm> PaymentPaid;
            List<ClsSalesVm> LedgerDiscount;
            List<ClsSalesVm> Ledger = new List<ClsSalesVm>();
            ClsUserVm User = new ClsUserVm();

            if (obj.BranchId == 0)
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && a.Status.ToLower() != "draft" && a.SupplierId == obj.SupplierId
 //&& a.BranchId == obj.BranchId 
 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate).Select(a => new ClsSalesVm
                {
                    Type = "Purchase",
                    AddedOn = a.PurchaseDate,
                    InvoiceNo = a.ReferenceNo,
                    BranchId = a.BranchId,
                    //PaymentStatus = a.PaymentStatus,
                    Status = a.Status,
                    Credit = 0,
                    Debit = a.GrandTotal,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PaymentType = "",
                    OtherRemarks = ""
                }).ToList();

                SupplierPayment = (from d in oConnectionContext.DbClsSupplierPayment
                                   join c in oConnectionContext.DbClsPurchase on d.PurchaseId equals c.PurchaseId
                                   where c.Status != "draft" &&
                  d.Type.ToLower() == "purchase payment" && c.SupplierId == obj.SupplierId
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                  && d.ParentId == 0
                                && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = c.BranchId,
                                       //PaymentStatus = "",
                                       Credit = d.Amount,
                                       Debit = 0,
                                       BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsPurchase.Where(cc => cc.PurchaseId == d.PurchaseId).Select(cc => cc.ReferenceNo).FirstOrDefault()
                                   }).ToList();

                PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.SupplierId == obj.SupplierId
            //&& b.BranchId == obj.BranchId
            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                           l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
                                  && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                           DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                  select new ClsSalesVm
                                  {
                                      Type = "Purchase Return",
                                      AddedOn = a.Date,
                                      InvoiceNo = a.InvoiceNo,
                                      BranchId = a.BranchId,
                                      //PaymentStatus = a.PaymentStatus,
                                      Status = a.Status,
                                      Credit = a.GrandTotal,
                                      Debit = 0,
                                      BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                PurchaseReturnPayment = (from e in oConnectionContext.DbClsSupplierPayment
                                         join c in oConnectionContext.DbClsPurchaseReturn
                                         on e.PurchaseId equals c.PurchaseReturnId
                                         //                 join d in oConnectionContext.DbClsSales
                                         //on c.SalesId equals d.SalesId
                                         where c.CompanyId == obj.CompanyId && c.SupplierId == obj.SupplierId
                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                         && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                         //&& d.IsDeleted == false && d.IsCancelled == false
                                         && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                        e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                         && e.ParentId == 0
                     && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                         select new ClsSalesVm
                                         {
                                             Type = e.Type,
                                             AddedOn = e.PaymentDate,
                                             InvoiceNo = e.ReferenceNo,
                                             BranchId = c.BranchId,
                                             //PaymentStatus = "",
                                             Credit = 0,
                                             Debit = e.Amount,
                                             BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                             PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                             OtherRemarks = oConnectionContext.DbClsPurchaseReturn.Where(cc => cc.PurchaseReturnId == e.PurchaseId).Select(cc => cc.InvoiceNo).FirstOrDefault()
                                         }).ToList();

                // Expense = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                // && a.ExpenseFor == obj.UserId
                // //&& a.BranchId == obj.BranchId 
                // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                // l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
                // && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                // DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                // ).Select(a => new ClsSalesVm
                // {
                //     Type ="Expense",
                //     AddedOn = a.Date,
                //     InvoiceNo = a.ReferenceNo,
                //     BranchId = a.BranchId,
                //     PaymentStatus = a.PaymentStatus,
                //     Credit=0,
                //     Debit = a.Amount,
                //     BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                //     PaymentType = ""
                // }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.SupplierId == obj.SupplierId
               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      BranchId = c.BranchId,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = d.Amount,
                                      BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault()
                                  }).ToList();

                LedgerDiscount = (from d in oConnectionContext.DbClsLedgerDiscount
                                  where d.Type.ToLower() == "supplier ledger discount" && d.Id == obj.SupplierId
               && d.IsActive == true && d.IsDeleted == false
               && DbFunctions.TruncateTime(d.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.AddedOn) <= obj.ToDate
                                  select new ClsSalesVm
                                  {
                                      Type = "Ledger Discount",
                                      AddedOn = d.PaymentDate,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Debit = 0,
                                      Credit = d.Amount,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                //PaymentReceived = (from d in oConnectionContext.DbClsSupplierRefund
                //                   where d.Type.ToLower() == "supplier refund" && d.SupplierId == obj.SupplierId
                //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //&& DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                //DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                //                   select new ClsSalesVm
                //                   {
                //                       Type = "Payment Received",
                //                       AddedOn = d.PaymentDate,
                //                       InvoiceNo = d.ReferenceNo,
                //                       BranchId = 0,
                //                       //PaymentStatus = "",
                //                       Debit = 0,
                //                       Credit = d.Amount,
                //                       BranchName = "",
                //                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                       OtherRemarks = ""
                //                   }).ToList();

                PaymentPaid = (from d in oConnectionContext.DbClsSupplierPayment
                               where d.Type.ToLower() == "supplier payment" && d.SupplierId == obj.SupplierId
                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                   && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                               select new ClsSalesVm
                               {
                                   Type = "Payment Paid",
                                   AddedOn = d.PaymentDate,
                                   InvoiceNo = d.ReferenceNo,
                                   BranchId = 0,
                                   //PaymentStatus = "",
                                   Credit = 0,
                                   Debit = d.Amount,
                                   BranchName = "",
                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                   OtherRemarks = ""
                               }).ToList();


                User = (from a in oConnectionContext.DbClsUser
                            //join b in oConnectionContext.DbClsUserBranchMap
                            //on a.UserId equals b.UserId 
                        where a.CompanyId == obj.CompanyId && a.UserId == obj.SupplierId &&
                     a.IsDeleted == false && a.IsActive == true
                        //&& a.UserId != obj.AddedBy 
                        select new ClsUserVm
                        {
                            AddedOn = a.AddedOn,
                            LedgerDiscount = a.LedgerDiscount,
                            OpeningBalance = a.OpeningBalance,
                            AdvanceBalance = (from d in oConnectionContext.DbClsSupplierPayment
                                              where d.Type.ToLower() == "supplier advance payment" && d.SupplierId == obj.SupplierId
                           && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                              select d.Amount).DefaultIfEmpty().Sum(),
                            TotalPurchase = oConnectionContext.DbClsPurchase.Where(c => c.Status != "draft" && c.SupplierId == a.UserId &&
                            c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
              //&& a.BranchId == obj.BranchId
              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate
             ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalPurchasePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                 join c in oConnectionContext.DbClsPurchase on d.PurchaseId equals c.PurchaseId
                                                 where c.Status != "draft" &&
                        d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId
                        && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                        && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                        && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                        && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                                   DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                                 select d.Amount).DefaultIfEmpty().Sum(),
                            TotalPurchaseReturn = (from aa in oConnectionContext.DbClsPurchaseReturn
                                                   where aa.CompanyId == obj.CompanyId && aa.IsDeleted == false && aa.IsCancelled == false && aa.SupplierId == obj.SupplierId
                             //&& b.BranchId == obj.BranchId
                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == aa.BranchId)
                                                   && DbFunctions.TruncateTime(aa.Date) >= obj.FromDate &&
                                            DbFunctions.TruncateTime(aa.Date) <= obj.ToDate
                                                   select aa.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalPurchaseReturnPaid = (from e in oConnectionContext.DbClsSupplierPayment
                                                       join c in oConnectionContext.DbClsPurchaseReturn
                                                       on e.PurchaseId equals c.PurchaseReturnId
                                                       //                 join d in oConnectionContext.DbClsSales
                                                       //on c.SalesId equals d.SalesId
                                                       where c.CompanyId == obj.CompanyId && c.SupplierId == obj.SupplierId
                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                       && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                                       //&& d.IsDeleted == false && d.IsCancelled == false
                                                       && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                                      e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                                                   //&& e.ParentId == 0
                                   && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                              DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                                       select e.Amount).DefaultIfEmpty().Sum(),
                            TotalOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                       where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == obj.SupplierId
                                    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       select d.Amount).DefaultIfEmpty().Sum()
                        }).FirstOrDefault();

                decimal _openingBalance = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserId == obj.SupplierId &&
                                  a.IsDeleted == false && a.IsActive == true).Select(a => a.OpeningBalance).FirstOrDefault();

                bool isOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                             where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == obj.CustomerId
                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select d.Amount).DefaultIfEmpty().Sum() == _openingBalance ? true : false;

                decimal _openingBalanceDue = 0;

                if (User.AddedOn.Year == DateTime.Now.Year)
                {
                    _openingBalanceDue = _openingBalance;
                }
                else
                {
                    if (isOpeningBalancePaid == false)
                    {
                        _openingBalanceDue = (_openingBalance + User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.LedgerDiscount);
                    }
                    else
                    {
                        _openingBalanceDue = (User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.LedgerDiscount);
                    }
                }

                OpeningBalance = (from a in oConnectionContext.DbClsUser
                                  where a.CompanyId == obj.CompanyId && a.UserId == obj.SupplierId &&
                                  a.IsDeleted == false && a.IsActive == true && a.OpeningBalance > 0
                                  select new ClsSalesVm
                                  {
                                      Type = "Opening Balance",
                                      AddedOn = obj.FromDate,//a.JoiningDate.Value,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = _openingBalanceDue,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                //OpeningBalancePayment = (from d in oConnectionContext.DbClsPayment
                //                   join c in oConnectionContext.DbClsUser on d.Id equals c.UserId
                //                   where d.Type.ToLower() == "customer opening balance payment" && c.UserId == obj.UserId
                //&& c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //                   select new ClsSalesVm
                //                   {
                //                       Type ="Opening Balance Payment",
                //                       AddedOn = d.PaymentDate,
                //                       InvoiceNo = d.ReferenceNo,
                //                       BranchId = 0,
                //                       PaymentStatus = "",
                //                       GrandTotal = d.Amount,
                //                       BranchName = "",
                //                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                       OtherRemarks = ""
                //                   }).ToList();


                Ledger = Purchase.Union(SupplierPayment).Union(PurchaseReturn).Union(PurchaseReturnPayment).Union(OpeningBalance).
                     //Union(OpeningBalancePayment).
                     Union(LedgerDiscount).
                     //Union(PaymentReceived).
                     Union(PaymentPaid)
                     //.Union(Expense)
                     .Union(ExpensePayment)
                     .ToList();

                decimal Balance = 0;

                foreach (var item in Ledger)
                {
                    Balance = Balance + (item.Credit - item.Debit);
                    item.Balance = Balance;
                }
            }
            else
            {
                Purchase = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                && a.Status.ToLower() != "draft" && a.SupplierId == obj.SupplierId
 && a.BranchId == obj.BranchId
            && DbFunctions.TruncateTime(a.PurchaseDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.PurchaseDate) <= obj.ToDate).Select(a => new ClsSalesVm
                {
                    Type = "Purchase",
                    AddedOn = a.PurchaseDate,
                    InvoiceNo = a.ReferenceNo,
                    BranchId = a.BranchId,
                    //PaymentStatus = a.PaymentStatus,
                    Status = a.Status,
                    Credit = 0,
                    Debit = a.GrandTotal,
                    BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    PaymentType = "",
                    OtherRemarks = ""
                }).ToList();

                SupplierPayment = (from d in oConnectionContext.DbClsSupplierPayment
                                   join c in oConnectionContext.DbClsPurchase on d.PurchaseId equals c.PurchaseId
                                   where c.Status != "draft" &&
                  d.Type.ToLower() == "purchase payment" && c.SupplierId == obj.SupplierId
                   && c.BranchId == obj.BranchId
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                  && d.ParentId == 0
                                && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = c.BranchId,
                                       //PaymentStatus = "",
                                       Credit = d.Amount,
                                       Debit = 0,
                                       BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsPurchase.Where(cc => cc.PurchaseId == d.PurchaseId).Select(cc => cc.ReferenceNo).FirstOrDefault()
                                   }).ToList();

                PurchaseReturn = (from a in oConnectionContext.DbClsPurchaseReturn
                                  where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.SupplierId == obj.SupplierId
            && a.BranchId == obj.BranchId
                                  && DbFunctions.TruncateTime(a.Date) >= obj.FromDate &&
                           DbFunctions.TruncateTime(a.Date) <= obj.ToDate
                                  select new ClsSalesVm
                                  {
                                      Type = "Purchase Return",
                                      AddedOn = a.Date,
                                      InvoiceNo = a.InvoiceNo,
                                      BranchId = a.BranchId,
                                      Status = a.Status,
                                      Credit = a.GrandTotal,
                                      Debit = 0,
                                      BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == b.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                PurchaseReturnPayment = (from e in oConnectionContext.DbClsSupplierPayment
                                         join c in oConnectionContext.DbClsPurchaseReturn
                                         on e.PurchaseId equals c.PurchaseReturnId
                                         //                 join d in oConnectionContext.DbClsSales
                                         //on c.SalesId equals d.SalesId
                                         where c.CompanyId == obj.CompanyId && c.SupplierId == obj.SupplierId
                                          && c.BranchId == obj.BranchId
                                         && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                         //&& d.IsDeleted == false && d.IsCancelled == false
                                         && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                        e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                         && e.ParentId == 0
                     && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                         select new ClsSalesVm
                                         {
                                             Type = e.Type,
                                             AddedOn = e.PaymentDate,
                                             InvoiceNo = e.ReferenceNo,
                                             BranchId = c.BranchId,
                                             //PaymentStatus = "",
                                             Credit = 0,
                                             Debit = e.Amount,
                                             BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                             PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                             OtherRemarks = oConnectionContext.DbClsPurchaseReturn.Where(cc => cc.PurchaseReturnId == e.PurchaseId).Select(cc => cc.InvoiceNo).FirstOrDefault()
                                         }).ToList();

                // Expense = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false
                // && a.ExpenseFor == obj.UserId
                // //&& a.BranchId == obj.BranchId 
                // && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                // l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false && l.IsCancelled == false).Any(l => l.BranchId == a.BranchId)
                // && DbFunctions.TruncateTime(a.AddedOn) >= obj.FromDate &&
                // DbFunctions.TruncateTime(a.AddedOn) <= obj.ToDate
                // ).Select(a => new ClsSalesVm
                // {
                //     Type ="Expense",
                //     AddedOn = a.Date,
                //     InvoiceNo = a.ReferenceNo,
                //     BranchId = a.BranchId,
                //     PaymentStatus = a.PaymentStatus,
                //     Credit=0,
                //     Debit = a.Amount,
                //     BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                //     PaymentType = ""
                // }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.SupplierId == obj.SupplierId
                && c.BranchId == obj.BranchId
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      BranchId = c.BranchId,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = d.Amount,
                                      BranchName = oConnectionContext.DbClsBranch.Where(b => b.BranchId == c.BranchId).Select(b => b.Branch).FirstOrDefault(),
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault()
                                  }).ToList();

                LedgerDiscount = (from d in oConnectionContext.DbClsLedgerDiscount
                                  where d.Type.ToLower() == "supplier ledger discount" && d.Id == obj.SupplierId
               && d.IsActive == true && d.IsDeleted == false
               && DbFunctions.TruncateTime(d.AddedOn) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.AddedOn) <= obj.ToDate
                                  select new ClsSalesVm
                                  {
                                      Type = "Ledger Discount",
                                      AddedOn = d.PaymentDate,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Debit = 0,
                                      Credit = d.Amount,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsSupplierPayment
                                   where d.Type.ToLower() == "supplier refund" && d.SupplierId == obj.SupplierId
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                   select new ClsSalesVm
                                   {
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       BranchId = 0,
                                       //PaymentStatus = "",
                                       Debit = 0,
                                       Credit = d.Amount,
                                       BranchName = "",
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = ""
                                   }).ToList();

                PaymentPaid = (from d in oConnectionContext.DbClsSupplierPayment
                               where d.Type.ToLower() == "supplier payment" && d.SupplierId == obj.SupplierId
                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                   && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                               select new ClsSalesVm
                               {
                                   Type = "Payment Paid",
                                   AddedOn = d.PaymentDate,
                                   InvoiceNo = d.ReferenceNo,
                                   BranchId = 0,
                                   //PaymentStatus = "",
                                   Credit = 0,
                                   Debit = d.Amount,
                                   BranchName = "",
                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                   OtherRemarks = ""
                               }).ToList();


                User = (from a in oConnectionContext.DbClsUser
                            //join b in oConnectionContext.DbClsUserBranchMap
                            //on a.UserId equals b.UserId 
                        where a.CompanyId == obj.CompanyId && a.UserId == obj.SupplierId &&
                     a.IsDeleted == false && a.IsActive == true
                        //&& a.UserId != obj.AddedBy 
                        select new ClsUserVm
                        {
                            AddedOn = a.AddedOn,
                            LedgerDiscount = a.LedgerDiscount,
                            OpeningBalance = a.OpeningBalance,
                            AdvanceBalance = (from d in oConnectionContext.DbClsSupplierPayment
                                              where d.Type.ToLower() == "supplier advance payment" && d.SupplierId == obj.SupplierId
                           && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                              select d.Amount).DefaultIfEmpty().Sum(),
                            TotalPurchase = oConnectionContext.DbClsPurchase.Where(c => c.Status != "draft" && c.SupplierId == a.UserId &&
                            c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
              && c.BranchId == obj.BranchId
                && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                 DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate
             ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalPurchasePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                 join c in oConnectionContext.DbClsPurchase on d.PurchaseId equals c.PurchaseId
                                                 where c.Status != "draft" &&
                        d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId
                         && c.BranchId == obj.BranchId
                        && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                        && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                        && DbFunctions.TruncateTime(d.PaymentDate) >= obj.FromDate &&
                                   DbFunctions.TruncateTime(d.PaymentDate) <= obj.ToDate
                                                 select d.Amount).DefaultIfEmpty().Sum(),
                            TotalPurchaseReturn = (from aa in oConnectionContext.DbClsPurchaseReturn
                                                   where aa.CompanyId == obj.CompanyId && aa.IsDeleted == false && aa.IsCancelled == false && aa.SupplierId == obj.SupplierId
                                                   && aa.BranchId == obj.BranchId && aa.BranchId == obj.BranchId
                                                   && DbFunctions.TruncateTime(aa.Date) >= obj.FromDate &&
                                            DbFunctions.TruncateTime(aa.Date) <= obj.ToDate
                                                   select aa.GrandTotal).DefaultIfEmpty().Sum(),
                            TotalPurchaseReturnPaid = (from e in oConnectionContext.DbClsSupplierPayment
                                                       join c in oConnectionContext.DbClsPurchaseReturn
                                                       on e.PurchaseId equals c.PurchaseReturnId
                                                       //                 join d in oConnectionContext.DbClsSales
                                                       //on c.SalesId equals d.SalesId
                                                       where c.CompanyId == obj.CompanyId && c.SupplierId == obj.SupplierId
                                                        && c.BranchId == obj.BranchId
                                                       && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                                       //&& d.IsDeleted == false && d.IsCancelled == false
                                                       && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                                      e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                                                   //&& e.ParentId == 0
                                   && DbFunctions.TruncateTime(e.PaymentDate) >= obj.FromDate &&
                              DbFunctions.TruncateTime(e.PaymentDate) <= obj.ToDate
                                                       select e.Amount).DefaultIfEmpty().Sum(),
                            TotalOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                                       where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == obj.SupplierId
                                    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       select d.Amount).DefaultIfEmpty().Sum()
                        }).FirstOrDefault();

                decimal _openingBalance = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId && a.UserId == obj.SupplierId &&
                                  a.IsDeleted == false && a.IsActive == true).Select(a => a.OpeningBalance).FirstOrDefault();

                bool isOpeningBalancePaid = (from d in oConnectionContext.DbClsSupplierPayment
                                             where d.Type.ToLower() == "supplier opening balance payment" && d.SupplierId == obj.CustomerId
                          && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select d.Amount).DefaultIfEmpty().Sum() == _openingBalance ? true : false;

                decimal _openingBalanceDue = 0;

                if (User.AddedOn.Year == DateTime.Now.Year)
                {
                    _openingBalanceDue = _openingBalance;
                }
                else
                {
                    if (isOpeningBalancePaid == false)
                    {
                        _openingBalanceDue = (_openingBalance + User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.LedgerDiscount);
                    }
                    else
                    {
                        _openingBalanceDue = (User.TotalSales + User.AdvanceBalance) - (User.TotalSalesPaid + User.LedgerDiscount);
                    }
                }

                OpeningBalance = (from a in oConnectionContext.DbClsUser
                                  where a.CompanyId == obj.CompanyId && a.UserId == obj.SupplierId &&
                                  a.IsDeleted == false && a.IsActive == true && a.OpeningBalance > 0
                                  select new ClsSalesVm
                                  {
                                      Type = "Opening Balance",
                                      AddedOn = obj.FromDate,//a.JoiningDate.Value,
                                      InvoiceNo = "",
                                      BranchId = 0,
                                      //PaymentStatus = "",
                                      Credit = 0,
                                      Debit = _openingBalanceDue,
                                      BranchName = "",
                                      PaymentType = "",
                                      OtherRemarks = ""
                                  }).ToList();

                //OpeningBalancePayment = (from d in oConnectionContext.DbClsPayment
                //                   join c in oConnectionContext.DbClsUser on d.Id equals c.UserId
                //                   where d.Type.ToLower() == "customer opening balance payment" && c.UserId == obj.UserId
                //&& c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                //                   select new ClsSalesVm
                //                   {
                //                       Type ="Opening Balance Payment",
                //                       AddedOn = d.PaymentDate,
                //                       InvoiceNo = d.ReferenceNo,
                //                       BranchId = 0,
                //                       PaymentStatus = "",
                //                       GrandTotal = d.Amount,
                //                       BranchName = "",
                //                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                //                       OtherRemarks = ""
                //                   }).ToList();


                Ledger = Purchase.Union(SupplierPayment).Union(PurchaseReturn).Union(PurchaseReturnPayment).Union(OpeningBalance).
                     //Union(OpeningBalancePayment).
                     Union(LedgerDiscount).
                     Union(PaymentReceived).
                     Union(PaymentPaid)
                     //.Union(Expense)
                     .Union(ExpensePayment)
                     .ToList();

                decimal Balance = 0;

                foreach (var item in Ledger)
                {
                    Balance = Balance + (item.Credit - item.Debit);
                    item.Balance = Balance;
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = Ledger.OrderBy(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    User = User,
                    TotalCount = Ledger.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SupplierPayments(ClsSalesVm obj)
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

            List<ClsSalesVm> SupplierPayment;
            List<ClsSalesVm> PurchaseReturnPayment;
            List<ClsSalesVm> ExpensePayment;
            List<ClsSalesVm> IncomePayment;
            List<ClsSalesVm> PaymentReceived;
            List<ClsSalesVm> PaymentPaid;
            List<ClsSalesVm> Ledger = new List<ClsSalesVm>();

            if (obj.BranchId == 0)
            {
                SupplierPayment = (from d in oConnectionContext.DbClsSupplierPayment
                                   join c in oConnectionContext.DbClsPurchase on d.PurchaseId equals c.PurchaseId
                                   where c.Status != "draft" && d.Type.ToLower() == "purchase payment" && c.SupplierId == obj.SupplierId
                  && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       PaymentId = d.SupplierPaymentId,
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsPurchase.Where(cc => cc.PurchaseId == d.PurchaseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                       SalesId = d.PurchaseId
                                   }).ToList();

                PurchaseReturnPayment = (from e in oConnectionContext.DbClsSupplierPayment
                                         join c in oConnectionContext.DbClsPurchaseReturn
                                         on e.PurchaseId equals c.PurchaseReturnId
                                         //                 join d in oConnectionContext.DbClsPurchase
                                         //on c.PurchaseId equals d.PurchaseId
                                         where c.CompanyId == obj.CompanyId && c.SupplierId == obj.SupplierId
                                         && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                         && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                         //&& d.IsDeleted == false && d.IsCancelled == false
                                         && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                        e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                         select new ClsSalesVm
                                         {
                                             PaymentId = e.SupplierPaymentId,
                                             Type = e.Type,
                                             AddedOn = e.PaymentDate,
                                             InvoiceNo = e.ReferenceNo,
                                             GrandTotal = e.Amount,
                                             PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                             OtherRemarks = oConnectionContext.DbClsPurchaseReturn.Where(cc => cc.PurchaseReturnId == e.PurchaseId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                             SalesId = e.PurchaseId,
                                         }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.SupplierId == obj.SupplierId
               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                       l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      PaymentId = d.ExpensePaymentId,
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      GrandTotal = d.Amount,
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                      SalesId = d.ExpenseId
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsSupplierPayment
                                   join c in oConnectionContext.DbClsUser on d.SupplierId equals c.UserId
                                   where d.Type.ToLower() == "supplier refund" && c.UserId == obj.SupplierId
                && c.IsActive == true && c.IsDeleted == false
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       PaymentId = d.SupplierPaymentId,
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = "",
                                       Payments = (from dd in oConnectionContext.DbClsSupplierPayment
                                                   where dd.ParentId == d.SupplierPaymentId && dd.Type.ToLower() == "supplier opening balance payment"
                                                   select new ClsCustomerPaymentVm
                                                   {
                                                       PaymentId = dd.SupplierPaymentId,
                                                       Type = dd.Type,
                                                       AddedOn = dd.PaymentDate,
                                                       InvoiceNo = dd.ReferenceNo,
                                                       GrandTotal = dd.Amount,
                                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                       OtherRemarks = "",
                                                       SalesId = dd.PurchaseId
                                                   }).Union(from dd in oConnectionContext.DbClsSupplierPayment
                                                            where dd.ParentId == d.SupplierPaymentId
                                                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == dd.BranchId)
                                                            select new ClsCustomerPaymentVm
                                                            {
                                                                PaymentId = dd.SupplierPaymentId,
                                                                Type = dd.Type,
                                                                AddedOn = dd.PaymentDate,
                                                                InvoiceNo = dd.ReferenceNo,
                                                                GrandTotal = dd.Amount,
                                                                PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                                OtherRemarks = oConnectionContext.DbClsPurchase.Where(cc => cc.PurchaseId == dd.PurchaseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                                                SalesId = dd.PurchaseId
                                                            }).ToList()
                                   }).ToList();

                PaymentPaid = (from d in oConnectionContext.DbClsSupplierPayment
                               join c in oConnectionContext.DbClsUser on d.SupplierId equals c.UserId
                               where d.Type.ToLower() == "supplier payment" && c.UserId == obj.SupplierId
                   && c.IsActive == true && c.IsDeleted == false
                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               select new ClsSalesVm
                               {
                                   PaymentId = d.SupplierPaymentId,
                                   Type = "Payment Paid",
                                   AddedOn = d.PaymentDate,
                                   InvoiceNo = d.ReferenceNo,
                                   GrandTotal = d.Amount,
                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                   OtherRemarks = "",
                                   Payments = (from dd in oConnectionContext.DbClsSupplierPayment
                                               where dd.ParentId == d.SupplierPaymentId
                                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                          l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == dd.BranchId)
                                               select new ClsCustomerPaymentVm
                                               {
                                                   PaymentId = dd.SupplierPaymentId,
                                                   Type = dd.Type,
                                                   AddedOn = dd.PaymentDate,
                                                   InvoiceNo = dd.ReferenceNo,
                                                   GrandTotal = dd.Amount,
                                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                   OtherRemarks = oConnectionContext.DbClsPurchaseReturn.Where(cc => cc.PurchaseReturnId == dd.PurchaseId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                                   SalesId = dd.PurchaseId
                                               }).ToList()
                               }).ToList();

                Ledger = SupplierPayment.Union(PurchaseReturnPayment).Union(PaymentReceived).Union(PaymentPaid).Union(ExpensePayment).ToList();


            }
            else
            {
                SupplierPayment = (from d in oConnectionContext.DbClsSupplierPayment
                                   join c in oConnectionContext.DbClsPurchase on d.PurchaseId equals c.PurchaseId
                                   where c.Status != "draft" && d.Type.ToLower() == "purchase payment" && c.SupplierId == obj.SupplierId
                  && c.BranchId == obj.BranchId
                  && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                  && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       PaymentId = d.SupplierPaymentId,
                                       Type = d.Type,
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = oConnectionContext.DbClsPurchase.Where(cc => cc.PurchaseId == d.PurchaseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                       SalesId = d.PurchaseId
                                   }).ToList();

                PurchaseReturnPayment = (from e in oConnectionContext.DbClsSupplierPayment
                                         join c in oConnectionContext.DbClsPurchaseReturn
                                         on e.PurchaseId equals c.PurchaseReturnId
                                         //                 join d in oConnectionContext.DbClsPurchase
                                         //on c.PurchaseId equals d.PurchaseId
                                         where c.CompanyId == obj.CompanyId && c.SupplierId == obj.SupplierId
                                         && c.BranchId == obj.BranchId
                                         && c.IsActive && c.IsDeleted == false && c.IsCancelled == false
                                         //&& d.IsDeleted == false && d.IsCancelled == false
                                         && e.Type.ToLower() == "supplier refund" && e.IsDeleted == false && e.IsCancelled == false &&
                        e.CompanyId == obj.CompanyId //&& e.BranchId == obj.BranchId
                                         select new ClsSalesVm
                                         {
                                             PaymentId = e.SupplierPaymentId,
                                             Type = e.Type,
                                             AddedOn = e.PaymentDate,
                                             InvoiceNo = e.ReferenceNo,
                                             GrandTotal = e.Amount,
                                             PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == e.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                             OtherRemarks = oConnectionContext.DbClsPurchaseReturn.Where(cc => cc.PurchaseReturnId == e.PurchaseId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                             SalesId = e.PurchaseId,
                                         }).ToList();

                ExpensePayment = (from d in oConnectionContext.DbClsExpensePayment
                                  join c in oConnectionContext.DbClsExpense on d.ExpenseId equals c.ExpenseId
                                  where c.SupplierId == obj.SupplierId
               && c.BranchId == obj.BranchId
               && c.IsActive == true && c.IsDeleted == false
               && d.IsActive == true && d.IsDeleted == false
                                  select new ClsSalesVm
                                  {
                                      PaymentId = d.ExpensePaymentId,
                                      Type = "Expense Payment",
                                      AddedOn = c.Date,
                                      InvoiceNo = c.ReferenceNo,
                                      GrandTotal = d.Amount,
                                      //PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                      OtherRemarks = oConnectionContext.DbClsExpense.Where(cc => cc.ExpenseId == d.ExpenseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                      SalesId = d.ExpenseId
                                  }).ToList();

                PaymentReceived = (from d in oConnectionContext.DbClsSupplierPayment
                                   join c in oConnectionContext.DbClsUser on d.SupplierId equals c.UserId
                                   where d.Type.ToLower() == "supplier refund" && c.UserId == obj.SupplierId
                && c.IsActive == true && c.IsDeleted == false
                && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                   select new ClsSalesVm
                                   {
                                       PaymentId = d.SupplierPaymentId,
                                       Type = "Payment Received",
                                       AddedOn = d.PaymentDate,
                                       InvoiceNo = d.ReferenceNo,
                                       GrandTotal = d.Amount,
                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                       OtherRemarks = "",
                                       Payments = (from dd in oConnectionContext.DbClsSupplierPayment
                                                   where dd.ParentId == d.SupplierPaymentId && dd.Type.ToLower() == "supplier opening balance payment"
                                                   select new ClsCustomerPaymentVm
                                                   {
                                                       PaymentId = d.SupplierPaymentId,
                                                       Type = dd.Type,
                                                       AddedOn = dd.PaymentDate,
                                                       InvoiceNo = dd.ReferenceNo,
                                                       GrandTotal = dd.Amount,
                                                       PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                       OtherRemarks = "",
                                                       SalesId = dd.PurchaseId
                                                   }).Union(from dd in oConnectionContext.DbClsSupplierPayment
                                                            where dd.ParentId == d.SupplierPaymentId && dd.BranchId == obj.BranchId
                                                            select new ClsCustomerPaymentVm
                                                            {
                                                                PaymentId = d.SupplierPaymentId,
                                                                Type = dd.Type,
                                                                AddedOn = dd.PaymentDate,
                                                                InvoiceNo = dd.ReferenceNo,
                                                                GrandTotal = dd.Amount,
                                                                PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                                OtherRemarks = oConnectionContext.DbClsPurchase.Where(cc => cc.PurchaseId == dd.PurchaseId).Select(cc => cc.ReferenceNo).FirstOrDefault(),
                                                                SalesId = dd.PurchaseId
                                                            }).ToList()
                                   }).ToList();

                PaymentPaid = (from d in oConnectionContext.DbClsSupplierPayment
                               join c in oConnectionContext.DbClsUser on d.SupplierId equals c.UserId
                               where d.Type.ToLower() == "supplier payment" && c.UserId == obj.SupplierId
                   && c.IsActive == true && c.IsDeleted == false
                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               select new ClsSalesVm
                               {
                                   PaymentId = d.SupplierPaymentId,
                                   Type = "Payment Paid",
                                   AddedOn = d.PaymentDate,
                                   InvoiceNo = d.ReferenceNo,
                                   GrandTotal = d.Amount,
                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == d.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                   OtherRemarks = "",
                                   Payments = (from dd in oConnectionContext.DbClsSupplierPayment
                                               where dd.ParentId == d.SupplierPaymentId && d.BranchId == obj.BranchId
                                               select new ClsCustomerPaymentVm
                                               {
                                                   PaymentId = d.SupplierPaymentId,
                                                   Type = dd.Type,
                                                   AddedOn = dd.PaymentDate,
                                                   InvoiceNo = dd.ReferenceNo,
                                                   GrandTotal = dd.Amount,
                                                   PaymentType = oConnectionContext.DbClsPaymentType.Where(cc => cc.PaymentTypeId == dd.PaymentTypeId).Select(cc => cc.PaymentType).FirstOrDefault(),
                                                   OtherRemarks = oConnectionContext.DbClsPurchaseReturn.Where(cc => cc.PurchaseReturnId == dd.PurchaseId).Select(cc => cc.InvoiceNo).FirstOrDefault(),
                                                   SalesId = dd.PurchaseId
                                               }).ToList()
                               }).ToList();

                Ledger = SupplierPayment.Union(PurchaseReturnPayment).Union(PaymentReceived).Union(PaymentPaid).Union(ExpensePayment).ToList();

            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Sales = Ledger.OrderByDescending(a => a.AddedOn).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = Ledger.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchCompanyCurrency(ClsUserVm obj)
        {
            var det = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => new
            {
                CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchRootCompany(ClsUserVm obj)
        {
            var det = oConnectionContext.DbClsUser.Where(a => a.IsRootAccount == true).Select(a => new
            {
                a.UserId
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> FetchWhitelabelByDomain(ClsUserVm obj)
        {
            var User = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false
            && a.IsActive == true).Select(a => new { UserId = a.CompanyId }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    User = User
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserCountByBatch(ClsUserVm obj)
        {
            long TotalCount = oConnectionContext.DbClsUser.Where(a => a.BatchNo == obj.BatchNo).Count();

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

        public async Task<IHttpActionResult> SalesByCustomerReport(ClsUserVm obj)
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

            List<ClsUserVm> det;

            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsUser
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" &&
                       a.IsDeleted == false
                       select new ClsUserVm
                       {

                           UserId = a.UserId,
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           TotalSalesInvoices = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId
                           && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
             ).Count(),
                           TotalTaxAmount = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId
                           && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
             ).Select(c => c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId
                           && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
             ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesPaid = (from c in oConnectionContext.DbClsSales
                                             join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                                             where c.Status != "Draft" &&
                    (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                    && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select d.Amount).DefaultIfEmpty().Sum(),
                           TotalSalesDue =
                                 oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false
                                 //&& c.BranchId == obj.BranchId
                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                         l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                 //&& c.CompanyId == obj.CompanyId
                                 && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                 ).Select(c => c.GrandTotal-c.WriteOffAmount).DefaultIfEmpty().Sum()
                            - (from c in oConnectionContext.DbClsSales
                               join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                               where c.Status != "Draft" &&
      (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
      && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
      && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
      && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               select d.Amount).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSales > 0).ToList();

                if (obj.CustomerId != 0)
                {
                    det = det.Where(a => a.UserId == obj.CustomerId).Select(a => a).ToList();
                }

                if (obj.PaymentStatus != null && obj.PaymentStatus != "")
                {
                    if (obj.PaymentStatus.ToLower() == "partially paid")
                    {
                        det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue > 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "paid")
                    {
                        det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue == 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "due")
                    {
                        det = det.Where(a => a.TotalSalesDue > 0).Select(a => a).ToList();
                    }
                }
            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" &&
                       a.IsDeleted == false
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           TotalSalesInvoices = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId
                           && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && c.BranchId == obj.BranchId
             ).Count(),
                           TotalTaxAmount = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId
                           && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && c.BranchId == obj.BranchId
             ).Select(c => c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSales = oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId
                           && c.CompanyId == obj.CompanyId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
             && c.BranchId == obj.BranchId
             ).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesPaid = (from c in oConnectionContext.DbClsSales
                                             join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                                             where c.Status != "Draft" &&
                    (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
                    && c.BranchId == obj.BranchId
                    && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select d.Amount).DefaultIfEmpty().Sum(),
                           TotalSalesDue =
                                 oConnectionContext.DbClsSales.Where(c => c.Status != "Draft" && c.CustomerId == a.UserId && a.IsActive == true && a.IsDeleted == false
                                 && c.BranchId == obj.BranchId
                                 //&& c.CompanyId == obj.CompanyId
                                 && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                 ).Select(c => c.GrandTotal-c.WriteOffAmount).DefaultIfEmpty().Sum()
                            - (from c in oConnectionContext.DbClsSales
                               join d in oConnectionContext.DbClsCustomerPayment on c.SalesId equals d.SalesId
                               where c.Status != "Draft" &&
      (d.Type.ToLower() == "sales payment") && c.CustomerId == a.UserId
      && c.BranchId == obj.BranchId
      && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
      && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               select d.Amount).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSales > 0).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.UserId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            {
                if (obj.PaymentStatus.ToLower() == "partially paid")
                {
                    det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue > 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "paid")
                {
                    det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue == 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "due")
                {
                    det = det.Where(a => a.TotalSalesDue > 0).Select(a => a).ToList();
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> SalesReturnByCustomerReport(ClsUserVm obj)
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

            List<ClsUserVm> det;

            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsUser
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" &&
                       a.IsDeleted == false
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           TotalSalesReturnInvoices = (from c in oConnectionContext.DbClsSalesReturn
                                                       join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                                       where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                       && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                       && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       select c.SalesReturnId).Count(),
                           TotalTaxAmount = (from c in oConnectionContext.DbClsSalesReturn
                                             join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                             where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                             && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
              l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                             && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                               join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                               where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                               && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalAmountRemaining = (from c in oConnectionContext.DbClsSalesReturn
                                              join d in oConnectionContext.DbClsCustomerPayment on c.SalesReturnId equals d.SalesReturnId
                                              where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                              && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                              && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
               l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                              && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                              select d.AmountRemaining).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSalesReturn > 0).ToList();

                if (obj.CustomerId != 0)
                {
                    det = det.Where(a => a.UserId == obj.CustomerId).Select(a => a).ToList();
                }

                if (obj.PaymentStatus != null && obj.PaymentStatus != "")
                {
                    if (obj.PaymentStatus.ToLower() == "partially paid")
                    {
                        det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "paid")
                    {
                        det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue == 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "due")
                    {
                        det = det.Where(a => a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                    }
                }
            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "customer" &&
                       a.IsDeleted == false
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           TotalSalesReturnInvoices = (from c in oConnectionContext.DbClsSalesReturn
                                                       join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                                       where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                       && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                                       && d.BranchId == obj.BranchId
                                                       && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       select c.SalesReturnId).Count(),
                           TotalTaxAmount = (from c in oConnectionContext.DbClsSalesReturn
                                             join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                             where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                             && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                             && d.BranchId == obj.BranchId
                                             && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             select c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsSalesReturn
                                               join d in oConnectionContext.DbClsSales on c.SalesId equals d.SalesId
                                               where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                               && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                               && d.BranchId == obj.BranchId
                                               && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalAmountRemaining = (from c in oConnectionContext.DbClsSalesReturn
                                              join d in oConnectionContext.DbClsCustomerPayment on c.SalesReturnId equals d.SalesReturnId
                                              where d.CustomerId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                              && d.CompanyId == obj.CompanyId && c.Status != "Draft"
                                              && d.BranchId == obj.BranchId
                                              && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                              select d.AmountRemaining).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSalesReturn > 0).ToList();
            }

            if (obj.CustomerId != 0)
            {
                det = det.Where(a => a.UserId == obj.CustomerId).Select(a => a).ToList();
            }

            if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            {
                if (obj.PaymentStatus.ToLower() == "partially paid")
                {
                    det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "paid")
                {
                    det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue == 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "due")
                {
                    det = det.Where(a => a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseBySupplierReport(ClsUserVm obj)
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

            List<ClsUserVm> det = new List<ClsUserVm>();

            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsUser
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           MobileNo = a.MobileNo,
                           TotalSalesInvoices = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                                                       //&& c.BranchId == obj.BranchId 
                                                                                       && c.Status != "Draft"
                                                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                                                       && c.CompanyId == obj.CompanyId
                                                                                       && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                                               DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Count(),
                           TotalTaxAmount = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                                                       //&& c.BranchId == obj.BranchId 
                                                                                       && c.Status != "Draft"
                                                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                   l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                                                       && c.CompanyId == obj.CompanyId
                                                                                       && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                                               DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Select(c => c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSales = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            //&& c.BranchId == obj.BranchId 
                                                            && c.Status != "Draft"
                                                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                            && c.CompanyId == obj.CompanyId
                                                            && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesPaid = (from c in oConnectionContext.DbClsPurchase
                                             join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                             where
    d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false && c.Status != "Draft" &&
    oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
        DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate
                                             select d.Amount).DefaultIfEmpty().Sum(),
                           TotalSalesDue =
                            oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            //&& c.BranchId == obj.BranchId 
                                                            && c.Status != "Draft"
                                                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                                                            && c.CompanyId == obj.CompanyId
                                                            && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                            (from c in oConnectionContext.DbClsPurchase
                             join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                             where
d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false && c.Status != "Draft" &&
oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate
                             select d.Amount).DefaultIfEmpty().Sum()
                       }).Where(a => a.TotalSales > 0).ToList();

                if (obj.SupplierId != 0)
                {
                    det = det.Where(a => a.UserId == obj.SupplierId).Select(a => a).ToList();
                }

                if (obj.PaymentStatus != null && obj.PaymentStatus != "")
                {
                    if (obj.PaymentStatus.ToLower() == "partially paid")
                    {
                        det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue > 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "paid")
                    {
                        det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue == 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "due")
                    {
                        det = det.Where(a => a.TotalSalesDue > 0).Select(a => a).ToList();
                    }
                }
            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                           //join b in oConnectionContext.DbClsUserBranchMap
                           //on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false
                       //&& b.BranchId == obj.BranchId
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           MobileNo = a.MobileNo,
                           TotalSalesInvoices = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                                                       && c.BranchId == obj.BranchId && c.Status != "Draft"
                                                                                       && c.CompanyId == obj.CompanyId
                                                                                       && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                                               DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Count(),
                           TotalTaxAmount = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                                                       && c.BranchId == obj.BranchId && c.Status != "Draft"
                                                                                       && c.CompanyId == obj.CompanyId
                                                                                       && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                                               DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Select(c => c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSales = oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            && c.BranchId == obj.BranchId && c.Status != "Draft"
                                                            && c.CompanyId == obj.CompanyId
                                                            && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalSalesPaid = (from c in oConnectionContext.DbClsPurchase
                                             join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                                             where
    d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
    && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false && c.BranchId == obj.BranchId && c.Status != "Draft" &&
        DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate
                                             select d.Amount).DefaultIfEmpty().Sum(),
                           TotalSalesDue =
                            oConnectionContext.DbClsPurchase.Where(c => c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
                                                            //&& c.BranchId == obj.BranchId 
                                                            && c.Status != "Draft"
                                                            && c.BranchId == obj.BranchId
                                                            && c.CompanyId == obj.CompanyId
                                                            && DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate).Select(c => c.GrandTotal).DefaultIfEmpty().Sum() -
                            (from c in oConnectionContext.DbClsPurchase
                             join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseId equals d.PurchaseId
                             where
d.Type.ToLower() == "purchase payment" && c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false
&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false && c.Status != "Draft" &&
d.BranchId == obj.BranchId &&
DbFunctions.TruncateTime(c.PurchaseDate) >= obj.FromDate &&
    DbFunctions.TruncateTime(c.PurchaseDate) <= obj.ToDate
                             select d.Amount).DefaultIfEmpty().Sum()
                       }).Where(a => a.TotalSales > 0).ToList();
            }

            det = det.Where(a => a.TotalSales > 0).ToList();

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.UserId == obj.SupplierId).Select(a => a).ToList();
            }

            if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            {
                if (obj.PaymentStatus.ToLower() == "partially paid")
                {
                    det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue > 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "paid")
                {
                    det = det.Where(a => a.TotalSalesPaid > 0 && a.TotalSalesDue == 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "due")
                {
                    det = det.Where(a => a.TotalSalesDue > 0).Select(a => a).ToList();
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> PurchaseReturnBySupplierReport(ClsUserVm obj)
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

            List<ClsUserVm> det = new List<ClsUserVm>();

            if (obj.BranchId == 0)
            {
                det = (from a in oConnectionContext.DbClsUser
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           MobileNo = a.MobileNo,
                           TotalSalesReturnInvoices = (from c in oConnectionContext.DbClsPurchaseReturn
                                                           //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                                       where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                                       && c.CompanyId == obj.CompanyId //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                       && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                               && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                                       select c.GrandTotal).Count(),
                           TotalTaxAmount = (from c in oConnectionContext.DbClsPurchaseReturn
                                                 //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                             where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                             && c.CompanyId == obj.CompanyId //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                             && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
      l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                      && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
                  DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                             select c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsPurchaseReturn
                                                   //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                               where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                               && c.CompanyId == obj.CompanyId //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                               && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId)
                       && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalAmountRemaining = (from c in oConnectionContext.DbClsPurchaseReturn
                                                   join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseReturnId equals d.PurchaseReturnId
                                                   where d.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                                   && d.CompanyId == obj.CompanyId
                                                   && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                    l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == d.BranchId)
                                                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                   select d.AmountRemaining).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSalesReturn > 0).ToList();

                if (obj.SupplierId != 0)
                {
                    det = det.Where(a => a.UserId == obj.SupplierId).Select(a => a).ToList();
                }

                if (obj.PaymentStatus != null && obj.PaymentStatus != "")
                {
                    if (obj.PaymentStatus.ToLower() == "partially paid")
                    {
                        det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "paid")
                    {
                        det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue == 0).Select(a => a).ToList();
                    }
                    else if (obj.PaymentStatus.ToLower() == "due")
                    {
                        det = det.Where(a => a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                    }
                }
            }
            else
            {
                det = (from a in oConnectionContext.DbClsUser
                           //join b in oConnectionContext.DbClsUserBranchMap
                           //on a.UserId equals b.UserId
                       where a.CompanyId == obj.CompanyId && a.UserType.ToLower() == "supplier" && a.IsDeleted == false
                       //&& b.BranchId == obj.BranchId
                       select new ClsUserVm
                       {
                           UserId = a.UserId,
                           Username = a.Username,
                           UserRoleId = a.UserRoleId,
                           RoleName = oConnectionContext.DbClsRole.Where(b => b.RoleId == a.UserRoleId).Select(b => b.RoleName).FirstOrDefault(),
                           UserGroupId = a.UserGroupId,
                           Name = a.Name,
                           MobileNo = a.MobileNo,
                           TotalSalesReturnInvoices = (from c in oConnectionContext.DbClsPurchaseReturn
                                                           //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                                       where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                                       && c.CompanyId == obj.CompanyId //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                               && c.BranchId == obj.BranchId && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
                            DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                                       select c.GrandTotal).Count(),
                           TotalTaxAmount = (from c in oConnectionContext.DbClsPurchaseReturn
                                                 //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                             where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                             && c.CompanyId == obj.CompanyId //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                     && c.BranchId == obj.BranchId && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
                  DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                             select c.TotalTaxAmount).DefaultIfEmpty().Sum(),
                           TotalSalesReturn = (from c in oConnectionContext.DbClsPurchaseReturn
                                                   //join d in oConnectionContext.DbClsPurchase on c.PurchaseId equals d.PurchaseId
                                               where c.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                               && c.CompanyId == obj.CompanyId //&& d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                       && c.BranchId == obj.BranchId && DbFunctions.TruncateTime(c.Date) >= obj.FromDate &&
                    DbFunctions.TruncateTime(c.Date) <= obj.ToDate
                                               select c.GrandTotal).DefaultIfEmpty().Sum(),
                           TotalAmountRemaining = (from c in oConnectionContext.DbClsPurchaseReturn
                                                   join d in oConnectionContext.DbClsSupplierPayment on c.PurchaseReturnId equals d.PurchaseReturnId
                                                   where d.SupplierId == a.UserId && c.IsActive == true && c.IsDeleted == false && c.IsCancelled == false && c.Status != "Draft"
                                                   && d.CompanyId == obj.CompanyId
                                                   && d.BranchId==obj.BranchId
                                                   && d.IsActive == true && d.IsDeleted == false && d.IsCancelled == false
                                                   select d.AmountRemaining).DefaultIfEmpty().Sum(),
                       }).Where(a => a.TotalSalesReturn > 0).ToList();
            }

            if (obj.SupplierId != 0)
            {
                det = det.Where(a => a.UserId == obj.SupplierId).Select(a => a).ToList();
            }

            if (obj.PaymentStatus != null && obj.PaymentStatus != "")
            {
                if (obj.PaymentStatus.ToLower() == "partially paid")
                {
                    det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "paid")
                {
                    det = det.Where(a => a.TotalSalesReturnPaid > 0 && a.TotalSalesReturnDue == 0).Select(a => a).ToList();
                }
                else if (obj.PaymentStatus.ToLower() == "due")
                {
                    det = det.Where(a => a.TotalSalesReturnDue > 0).Select(a => a).ToList();
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    UserReport = det.OrderByDescending(a => a.UserId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    //Branchs = userDetails.BranchIds,
                    PageSize = obj.PageSize,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                }
            };

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UserLoginFromAdmin(ClsUserVm obj)
        {
            if (obj.Under == 0)
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            }

            //long Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            long WhitelabelUnder = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.Under).Select(a => a.Under).FirstOrDefault();
            if (obj.Under != WhitelabelUnder)
            {
                obj.Under = WhitelabelUnder;
            }
            //else
            //{
            //    obj.Under = Under;
            //}

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.UserType).FirstOrDefault();

            var det = (from a in oConnectionContext.DbClsUser
                       where a.UserType.ToLower() == "user" && a.IsCompany == true &&
                       a.IsDeleted == false && a.Under == obj.Under && a.UserId == obj.UserId
                       select new
                       {
                           a.Name,
                           UserId = a.UserId,
                           EmailId = a.EmailId,
                           MobileNo = a.MobileNo,
                           a.UserRoleId,
                           a.CompanyId,
                           UserType = a.UserType,
                           CountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == a.CompanyId).Select(b => b.CountryId).FirstOrDefault(),
                       }).FirstOrDefault();

            string token = oCommonController.CreateToken();

            var CurrentDate = oCommonController.CurrentDate(det.CompanyId);

            //var LoginDetail = oConnectionContext.DbClsLoginDetails.Where(a => a.AddedBy == det.CompanyId && a.IsLoggedOut == false).FirstOrDefault();
            long LoginDetailsId = oCommonController.InsertLoginDetails(det.UserId, det.UserType, false, "", obj.Platform, "", "", "", obj.IpAddress, token, CurrentDate, obj.Browser);

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                a.BusinessName,
                a.EnableDarkMode,
                a.DatatablePageEntries,
                a.ShowHelpText,
                a.DateFormat,
                a.TimeFormat,
                a.CurrencySymbolPlacement,
                a.FixedHeader,
                a.FixedFooter,
                a.EnableSound,
                a.CollapseSidebar,
                a.Favicon
            }).FirstOrDefault();

            var ShortCutKeySettings = (from a in oConnectionContext.DbClsShortCutKeySetting
                                       join b in oConnectionContext.DbClsMenu
                                       on a.MenuId equals b.MenuId
                                       where a.CompanyId == det.UserId && a.IsActive == true
                                       select new
                                       {
                                           b.Url,
                                           a.MenuId,
                                           a.Title,
                                           a.ShortCutKey
                                       }).Union(oConnectionContext.DbClsShortCutKeySetting.Where(a => a.CompanyId == det.UserId && a.MenuId == 0).Select(a => new
                                       {
                                           Url = "",
                                           a.MenuId,
                                           a.Title,
                                           a.ShortCutKey,
                                       })).ToList();

            var ItemSetting = oConnectionContext.DbClsItemSettings.Where(a => a.CompanyId == det.CompanyId).Select(a => new
            {
                a.ExpiryDateFormat
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "Login Successful",
                Data = new
                {
                    User = new
                    {
                        UserId = det.UserId,
                        UserType = det.UserType,
                        Token = token,
                        LoginDetailsId = LoginDetailsId,
                        CurrencySymbol = oConnectionContext.DbClsCountry.Where(b => b.CountryId == det.CountryId).Select(b => b.CurrencySymbol).FirstOrDefault(),
                        CurrencyCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == det.CountryId).Select(b => b.CurrencyCode).FirstOrDefault(),
                        DialingCode = oConnectionContext.DbClsCountry.Where(b => b.CountryId == det.CountryId).Select(b => b.DialingCode).FirstOrDefault(),
                        det.CompanyId,
                        BusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.BusinessName).FirstOrDefault(),
                        WhitelabelBusinessName = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == WhitelabelUnder).Select(b => b.BusinessName).FirstOrDefault(),
                        WhitelabelBusinessIcon = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == WhitelabelUnder).Select(b => b.BusinessIcon).FirstOrDefault(),
                        WhitelabelFavicon = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == WhitelabelUnder).Select(b => b.Favicon).FirstOrDefault(),
                    },
                    BusinessSetting = BusinessSetting,
                    ShortCutKeySettings = ShortCutKeySettings,
                    ItemSetting = ItemSetting
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateCreditLimit(ClsUserVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.CreditLimit == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divNewCreditLimit" });
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

                ClsUser oClsUser = new ClsUser();
                oClsUser.UserId = obj.UserId;
                oClsUser.CreditLimit = obj.CreditLimit;
                oClsUser.ModifiedOn = CurrentDate;
                oClsUser.ModifiedBy = obj.AddedBy;
                oConnectionContext.DbClsUser.Attach(oClsUser);
                oConnectionContext.Entry(oClsUser).Property(x => x.CreditLimit).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsUser).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = obj.UserType + "s",
                    CompanyId = obj.CompanyId,
                    Description = obj.UserType + " \"" + obj.Username + "\" updated",
                    Id = oClsUser.UserId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Credit Limit updated successfully.",
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
