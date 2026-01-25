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
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class BranchController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllBranchs(ClsBranchVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);


            var det = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                BranchId = a.BranchId,
                BranchCode = a.BranchCode,
                Branch = a.Branch,
                ContactName = a.ContactName,
                Name = oConnectionContext.DbClsUser.Where(b => b.UserId == a.ContactPersonId).Select(b => b.Name).FirstOrDefault(),
                Mobile = a.Mobile,
                Email = a.Email,
                CityId = a.CityId,
                a.IsMain,
                StateId = a.StateId,
                CountryId = a.CountryId,
                Address = a.Address,
                Zipcode = a.Zipcode,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                AddedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.AddedBy).Select(z => z.Username).FirstOrDefault(),
                ModifiedByCode = oConnectionContext.DbClsUser.Where(z => z.UserId == a.ModifiedBy).Select(z => z.Username).FirstOrDefault(),
                a.TaxId,
                a.TaxNo,
                a.EnableInlineTax,
            }).ToList();

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Branch.ToLower().Contains(obj.Search.ToLower()) || a.BranchCode.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Branchs = det.OrderByDescending(a => a.BranchId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Branch(ClsBranchVm obj)
        {
            var det = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.WhatsappNo,
                a.PrefixId,
                a.TaxSettingId,
                a.IsMain,
                BranchId = a.BranchId,
                BranchCode = a.BranchCode,
                Branch = a.Branch,
                ContactName = a.ContactName,
                Mobile = a.Mobile,
                a.AltMobileNo,
                Email = a.Email,
                CityId = a.CityId,
                StateId = a.StateId,
                CountryId = a.CountryId,
                Address = a.Address,
                Zipcode = a.Zipcode,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                PaymentTypes = oConnectionContext.DbClsPaymentType.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                && b.IsDeleted == false && b.IsAdvance == false).Select(b => new
                {
                    IsPosShown = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Select(c => c.IsPosShown).FirstOrDefault(),
                    AccountId = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
                    c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Select(c => c.AccountId).FirstOrDefault(),
                    b.PaymentType,
                    b.PaymentTypeId,
                    IsChecked = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
                    c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Count() == 0 ? false : true,
                    IsDefault = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Select(c => c.IsDefault).FirstOrDefault(),
                }).ToList(),
                a.TaxId,
                a.TaxNo,
                a.EnableInlineTax,
                a.ContactPersonId
            }).FirstOrDefault();

            var Countrys = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CountryId = a.CountryId,
                a.CountryCode,
                Country = a.Country,
            }).OrderBy(a => a.Country).ToList();

            var States = oConnectionContext.DbClsState.Where(a =>
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == det.CountryId
            ).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                State = a.State,
            }).OrderBy(a => a.State).ToList();

            var Citys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.CountryId == det.CountryId
            && a.StateId == det.StateId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                City = a.City,
            }).OrderBy(a => a.City).ToList();

            //var Currencys = oConnectionContext.DbClsCurrency.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    CurrencyId = a.CurrencyId,
            //    a.CurrencyCode,
            //    CurrencyName = a.CurrencyName,
            //}).OrderBy(a => a.CurrencyName).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Branch = det,
                    Countrys = Countrys,
                    States = States,
                    Citys = Citys,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertBranch(ClsBranchVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int TotalBranchUsed = oConnectionContext.DbClsBranch.AsEnumerable().Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count();
                int TotalBranch = oCommonController.fetchPlanQuantity(obj.CompanyId, "Branch");
                if (TotalBranchUsed >= TotalBranch)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Branch quota already used. Please upgrade addons from My Plan Menu",
                        Data = new
                        {
                        }
                    };
                    return await Task.FromResult(Ok(data));
                }

                long PrefixUserMapId = 0;
                //obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                if (obj.Branch == null || obj.Branch == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }
                //if (obj.ContactPersonId == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divContactPerson" });
                //    isError = true;
                //}
                if (obj.Mobile == "" || obj.Mobile == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                    isError = true;
                }
                if (obj.Mobile != null && obj.Mobile != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.Mobile);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Contact No", Id = "divMobileNo" });
                        isError = true;
                    }
                }
                if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Alternative Contact No", Id = "divAltMobileNo" });
                        isError = true;
                    }
                }
                if (obj.WhatsappNo != null && obj.WhatsappNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.WhatsappNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Whatsapp No", Id = "divWhatsappNo" });
                        isError = true;
                    }
                }
                if (obj.Email != null && obj.Email != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.Email);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }
                if (obj.PaymentTypes == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                    isError = true;
                }
                if (obj.CountryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCountry" });
                    isError = true;
                }
                else
                {
                    if (obj.CountryId == 2)
                    {
                        int IsBusinessRegistered = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.IsBusinessRegistered).FirstOrDefault();
                        if (IsBusinessRegistered == 1)
                        {
                            if (obj.TaxSettingId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divTaxSetting" });
                                isError = true;
                            }
                        }
                    }
                }

                if (obj.StateId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }
                if (obj.CityId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCity" });
                    isError = true;
                }
                if (obj.Zipcode == null || obj.Zipcode == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divZipcode" });
                    isError = true;
                }
                if (obj.Address == null || obj.Address == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAddress" });
                    isError = true;
                }

                if (obj.BranchCode != "" && obj.BranchCode != null)
                {
                    if (oConnectionContext.DbClsBranch.Where(a => a.BranchCode.ToLower() == obj.BranchCode.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Branch Code exists", Id = "divBranchCode" });
                        isError = true;
                    }
                }

                if (obj.Branch != "" && obj.Branch != null)
                {
                    if (oConnectionContext.DbClsBranch.Where(a => a.Branch.ToLower() == obj.Branch.ToLower() && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Duplicate Branch Name exists", Id = "divBranch" });
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

                //if (obj.BranchCode == "" || obj.BranchCode == null)
                //{
                //    long PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                //    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                //                          join b in oConnectionContext.DbClsPrefixUserMap
                //                           on a.PrefixMasterId equals b.PrefixMasterId
                //                          where a.IsActive == true && a.IsDeleted == false && a.IsCancelled == false &&
                //                          b.CompanyId == obj.CompanyId && b.IsActive == true
                //                          && b.IsDeleted == false && b.IsCancelled == false && a.PrefixType == "Branch"
                //                          && b.PrefixId == PrefixId
                //                          select new
                //                          {
                //                              b.PrefixUserMapId,
                //                              b.Prefix,
                //                              b.NoOfDigits,
                //                              b.Counter
                //                          }).FirstOrDefault();
                //    PrefixUserMapId = prefixSettings.PrefixUserMapId;
                //    obj.BranchCode = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                //}

                if (oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsMain == true).Count() == 0)
                {
                    obj.IsMain = true;
                }
                ClsBranch oBranch = new ClsBranch()
                {
                    AltMobileNo = obj.AltMobileNo,
                    BranchId = obj.BranchId,
                    BranchCode = obj.BranchCode,
                    Branch = obj.Branch,
                    ContactName = obj.ContactName,
                    Mobile = obj.Mobile,
                    Email = obj.Email,
                    CityId = obj.CityId,
                    StateId = obj.StateId,
                    CountryId = obj.CountryId,
                    Address = obj.Address,
                    Zipcode = obj.Zipcode,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                    IsMain = obj.IsMain,
                    ContactPersonId = obj.ContactPersonId,
                    TaxId = obj.TaxId,
                    TaxNo = obj.TaxNo,
                    EnableInlineTax = obj.EnableInlineTax,
                    TaxSettingId = obj.TaxSettingId,
                    PrefixId = obj.PrefixId,
                    WhatsappNo = obj.WhatsappNo
                };
                oConnectionContext.DbClsBranch.Add(oBranch);
                oConnectionContext.SaveChanges();

                ////Map with Payment Types
                if (obj.PaymentTypes != null)
                {
                    foreach (var item in obj.PaymentTypes)
                    {
                        ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap = new ClsBranchPaymentTypeMap()
                        {
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                            PaymentTypeId = item.PaymentTypeId,
                            CompanyId = obj.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            ModifiedBy = obj.AddedBy,
                            BranchId = oBranch.BranchId,
                            AccountId = item.AccountId,
                            IsPosShown = item.IsPosShown,
                            IsDefault = item.IsDefault
                        };
                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap);
                        oConnectionContext.SaveChanges();
                    }

                    //map advance payment with branch
                    long PaymentTypeId = oConnectionContext.DbClsPaymentType.Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true).Select(a => a.PaymentTypeId).FirstOrDefault();
                    ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap1 = new ClsBranchPaymentTypeMap()
                    {
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        PaymentTypeId = PaymentTypeId,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedBy = obj.AddedBy,
                        BranchId = oBranch.BranchId,
                        AccountId = 0,
                        IsPosShown = false,
                        IsDefault = false
                    };
                    //ConnectionContext ocon = new ConnectionContext();
                    oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap1);
                    oConnectionContext.SaveChanges();
                }

                if (obj.IsMain == true)
                {
                    string query = "update \"tblUser\" set \"BranchIds\"='" + oBranch.BranchId + "," + "' where \"UserId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                // map walkin customer with branch
                long UserId = oConnectionContext.DbClsUser.Where(a => a.CompanyId == obj.CompanyId &&
                a.UserType.ToLower() == "customer" && a.IsWalkin == true).Select(a => a.UserId).FirstOrDefault();

                ClsUserBranchMap oClsUserBranchMap = new ClsUserBranchMap()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    BranchId = oBranch.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    UserType = "Customer",
                    ModifiedBy = obj.AddedBy,
                    UserId = UserId
                };

                //ConnectionContext ocon = new ConnectionContext();
                oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap);
                oConnectionContext.SaveChanges();
                // map walkin customer with branch

                //map company with branch
                ClsUserBranchMap oClsUserBranchMap1 = new ClsUserBranchMap()
                {
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    BranchId = oBranch.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    UserType = "User",
                    ModifiedBy = obj.AddedBy,
                    UserId = obj.CompanyId
                };
                //ConnectionContext ocon = new ConnectionContext();
                oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap1);
                oConnectionContext.SaveChanges();
                //map company with branch

                // Auto-initialize Restaurant Settings for new branch
                ClsRestaurantSettings defaultRestaurantSettings = new ClsRestaurantSettings()
                {
                    EnableKitchenDisplay = true,
                    AutoPrintKot = true,
                    EnableTableBooking = true,
                    EnableRecurringBooking = false,
                    BookingAdvanceDays = 30,
                    DefaultBookingDuration = 120,
                    RequireDeposit = false,
                    DepositMode = "Fixed",
                    DepositFixedAmount = 0,
                    DepositPerGuestAmount = 0,
                    EnablePublicBooking = false,
                    PublicBookingSlug = null,
                    PublicBookingAdvanceDays = 30,
                    PublicBookingRequireDeposit = false,
                    PublicBookingDepositPercentage = 0,
                    PublicBookingDepositMode = "Fixed",
                    PublicBookingDepositFixedAmount = 0,
                    PublicBookingDepositPerGuestAmount = 0,
                    PublicBookingAutoConfirm = false,
                    EnablePublicBookingCancellation = true,
                    AllowCancelAfterConfirm = true,
                    PublicBookingCancellationDaysBefore = 0,
                    PublicBookingCancellationChargeMode = "None",
                    PublicBookingCancellationFixedCharge = 0,
                    PublicBookingCancellationPercentage = 0,
                    PublicBookingCancellationPerGuestCharge = 0,
                    BranchId = oBranch.BranchId,
                    CompanyId = obj.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy
                };
                oConnectionContext.DbClsRestaurantSettings.Add(defaultRestaurantSettings);
                oConnectionContext.SaveChanges();
                // Auto-initialize Restaurant Settings for new branch

                ////increase counter
                //string q = "update \"tblPrefixUserMap\" set \"Counter\" = \"Counter\"+1 where \"PrefixUserMapId\"=" + PrefixUserMapId;
                //oConnectionContext.Database.ExecuteSqlCommand(q);
                ////increase counter

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Branch",
                    CompanyId = obj.CompanyId,
                    Description = "Branch \"" + obj.BranchCode + "\" created",
                    Id = oBranch.BranchId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Branch created successfully",
                    Data = new
                    {
                        Branch = oBranch
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateBranch(ClsBranchVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                if (obj.Branch == null || obj.Branch == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }
                //if (obj.ContactPersonId == 0)
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divContactPerson" });
                //    isError = true;
                //}
                if (obj.Mobile == "" || obj.Mobile == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                    isError = true;
                }
                if (obj.Mobile != null && obj.Mobile != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.Mobile);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Contact No", Id = "divMobileNo" });
                        isError = true;
                    }
                }
                if (obj.WhatsappNo != null && obj.WhatsappNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.WhatsappNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Whatsapp No", Id = "divWhatsappNo" });
                        isError = true;
                    }
                }
                if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Alternative Contact No", Id = "divAltMobileNo" });
                        isError = true;
                    }
                }

                if (obj.Email != null && obj.Email != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.Email);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }
                if (obj.PaymentTypes == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                    isError = true;
                }
                if (obj.CountryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCountry" });
                    isError = true;
                }
                else
                {
                    if (obj.CountryId == 2)
                    {
                        int IsBusinessRegistered = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.IsBusinessRegistered).FirstOrDefault();
                        if (IsBusinessRegistered == 1)
                        {
                            if (obj.TaxSettingId == 0)
                            {
                                errors.Add(new ClsError { Message = "This field is required", Id = "divTaxSetting" });
                                isError = true;
                            }
                        }
                    }
                }

                if (obj.StateId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }
                if (obj.CityId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCity" });
                    isError = true;
                }
                if (obj.Zipcode == null || obj.Zipcode == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divZipcode" });
                    isError = true;
                }
                if (obj.Address == null || obj.Address == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAddress" });
                    isError = true;
                }

                if (oConnectionContext.DbClsBranch.Where(a => a.Branch.ToLower() == obj.Branch.ToLower() && a.BranchId != obj.BranchId
                && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                {
                    errors.Add(new ClsError { Message = "Duplicate Branch Name exists", Id = "divBranch" });
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

                ClsBranch oBranch = new ClsBranch()
                {
                    BranchId = obj.BranchId,
                    //BranchCode = obj.BranchCode,
                    Branch = obj.Branch,
                    ContactName = obj.ContactName,
                    Mobile = obj.Mobile,
                    AltMobileNo = obj.AltMobileNo,
                    Email = obj.Email,
                    CityId = obj.CityId,
                    StateId = obj.StateId,
                    CountryId = obj.CountryId,
                    Address = obj.Address,
                    Zipcode = obj.Zipcode,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    ContactPersonId = obj.ContactPersonId,
                    TaxId = obj.TaxId,
                    TaxNo = obj.TaxNo,
                    EnableInlineTax = obj.EnableInlineTax,
                    TaxSettingId = obj.TaxSettingId,
                    PrefixId = obj.PrefixId,
                    WhatsappNo = obj.WhatsappNo
                };
                oConnectionContext.DbClsBranch.Attach(oBranch);
                oConnectionContext.Entry(oBranch).Property(x => x.BranchId).IsModified = true;
                //oConnectionContext.Entry(oBranch).Property(x => x.BranchCode).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Branch).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ContactName).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Mobile).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.AltMobileNo).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Email).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.CityId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.StateId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Address).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Zipcode).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ContactPersonId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.TaxNo).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.EnableInlineTax).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.TaxSettingId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.PrefixId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.WhatsappNo).IsModified = true;
                oConnectionContext.SaveChanges();

                string query = "update \"tblBranchPaymentTypeMap\" set \"IsActive\"= False where \"BranchId\"=" + oBranch.BranchId;
                oConnectionContext.Database.ExecuteSqlCommand(query);
                ////Map with Payment Types
                if (obj.PaymentTypes != null)
                {
                    foreach (var innerpaymentType in obj.PaymentTypes)
                    {
                        long BranchPaymentTypeMapId = oConnectionContext.DbClsBranchPaymentTypeMap.Where(a => a.PaymentTypeId == innerpaymentType.PaymentTypeId &&
                        a.BranchId == obj.BranchId).Select(a => a.BranchPaymentTypeMapId).FirstOrDefault();

                        if (BranchPaymentTypeMapId == 0)
                        {
                            ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap = new ClsBranchPaymentTypeMap()
                            {
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                                PaymentTypeId = innerpaymentType.PaymentTypeId,
                                CompanyId = obj.CompanyId,
                                IsActive = true,
                                IsDeleted = false,
                                ModifiedBy = obj.AddedBy,
                                BranchId = oBranch.BranchId,
                                AccountId = innerpaymentType.AccountId,
                                IsPosShown = innerpaymentType.IsPosShown,
                                IsDefault = innerpaymentType.IsDefault
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsBranchPaymentTypeMap.Add(oClsBranchPaymentTypeMap);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {
                            ClsBranchPaymentTypeMap oClsBranchPaymentTypeMap = new ClsBranchPaymentTypeMap()
                            {
                                BranchPaymentTypeMapId = BranchPaymentTypeMapId,
                                PaymentTypeId = innerpaymentType.PaymentTypeId,
                                ModifiedBy = obj.AddedBy,
                                //BranchId = oBranch.BranchId,
                                ModifiedOn = CurrentDate,
                                IsActive = true,
                                AccountId = innerpaymentType.AccountId,
                                IsPosShown = innerpaymentType.IsPosShown,
                                IsDefault = innerpaymentType.IsDefault
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsBranchPaymentTypeMap.Attach(oClsBranchPaymentTypeMap);
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.PaymentTypeId).IsModified = true;
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.ModifiedBy).IsModified = true;
                            //oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.BranchId).IsModified = true;
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.IsActive).IsModified = true;
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.AccountId).IsModified = true;
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.IsPosShown).IsModified = true;
                            oConnectionContext.Entry(oClsBranchPaymentTypeMap).Property(x => x.IsDefault).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }

                ////map company with branch
                //ClsUserBranchMap oClsUserBranchMap1 = new ClsUserBranchMap()
                //{
                //    AddedBy = obj.AddedBy,
                //    AddedOn = CurrentDate,
                //    BranchId = obj.BranchId,
                //    CompanyId = obj.CompanyId,
                //    IsActive = true,
                //    IsDeleted = false,
                //    UserType = "User",
                //    ModifiedBy = obj.AddedBy,
                //    UserId = obj.CompanyId
                //};
                ////ConnectionContext ocon = new ConnectionContext();
                //oConnectionContext.DbClsUserBranchMap.Add(oClsUserBranchMap1);
                //oConnectionContext.SaveChanges();
                ////map company with branch

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Branch",
                    CompanyId = obj.CompanyId,
                    Description = "Branch \"" + oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.BranchCode).FirstOrDefault() + "\" updated",
                    Id = oBranch.BranchId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Branch updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BranchActiveInactive(ClsBranchVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                if (obj.IsActive == true)
                {
                    int TotalBranch = oCommonController.fetchPlanQuantity(obj.CompanyId, "Branch");
                    int TotalBranchUsed = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true).Count();
                    if (TotalBranchUsed >= TotalBranch)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Branch quota already used. Please upgrade addons from My Plan Menu",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                ClsBranch oClsRole = new ClsBranch()
                {
                    BranchId = obj.BranchId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBranch.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Branch",
                    CompanyId = obj.CompanyId,
                    Description = "Branch \"" + oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.BranchCode).FirstOrDefault() + (obj.IsActive == true ? "\" activated" : "\" deactivated"),
                    Id = oClsRole.BranchId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Branch " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BranchDelete(ClsBranchVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                //int UserCount = (from a in oConnectionContext.DbClsUser 
                //                 join b in oConnectionContext.DbClsUserBranchMap 
                //                 on a.UserId equals b.UserId 
                //                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.BranchId == obj.BranchId 
                //                 select a.UserId).Count();
                int ItemCount = (from a in oConnectionContext.DbClsItem
                                 join b in oConnectionContext.DbClsItemBranchMap
                                 on a.ItemId equals b.ItemId
                                 where a.CompanyId == obj.CompanyId && a.IsDeleted == false && b.IsDeleted == false && b.BranchId == obj.BranchId
                                 select a.ItemId).Count();
                int OpeningStockCount = oConnectionContext.DbClsOpeningStock.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int StockAdjustmentCount = oConnectionContext.DbClsStockAdjustment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int StockTransferCount = oConnectionContext.DbClsStockTransfer.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && (a.FromBranchId == obj.BranchId || a.ToBranchId == obj.BranchId)).Count();
                int PurchaseQuotationCount = oConnectionContext.DbClsPurchaseQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int PurchaseOrderCount = oConnectionContext.DbClsPurchaseOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int PurchaseCount = oConnectionContext.DbClsPurchase.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                int PurchaseReturnCount = oConnectionContext.DbClsPurchaseReturn.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                int SupplierPaymentCount = oConnectionContext.DbClsSupplierPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                //int SupplierRefundCount = oConnectionContext.DbClsSupplierRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                int ExpenseCount = oConnectionContext.DbClsExpense.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int SalesQuotationCount = oConnectionContext.DbClsSalesQuotation.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int SalesOrderCount = oConnectionContext.DbClsSalesOrder.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int SalesProformaCount = oConnectionContext.DbClsSalesProforma.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int DeliveryChallanCount = oConnectionContext.DbClsDeliveryChallan.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                int SalesCount = oConnectionContext.DbClsSales.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                int SalesReturnCount = (from a in oConnectionContext.DbClsSalesReturn
                                        join b in oConnectionContext.DbClsSales
                                        on a.SalesId equals b.SalesId
                                        where a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && b.IsDeleted == false && b.IsCancelled == false && b.BranchId == obj.BranchId
                                        select a.SalesId).Count();
                int CustomerPaymentCount = oConnectionContext.DbClsCustomerPayment.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                //int CustomerRefundCount = oConnectionContext.DbClsCustomerRefund.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                //int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.BranchId == obj.BranchId).Count();
                int ContraCount = oConnectionContext.DbClsContra.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();
                int JournalCount = oConnectionContext.DbClsJournal.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.BranchId == obj.BranchId).Count();

                if (
                    //UserCount > 0 || 
                    ItemCount > 0 || OpeningStockCount > 0 || StockAdjustmentCount > 0
                    || StockTransferCount > 0 || PurchaseQuotationCount > 0 || PurchaseOrderCount > 0 || PurchaseCount > 0
                    || PurchaseReturnCount > 0 || SupplierPaymentCount > 0
                    //|| SupplierRefundCount > 0
                    || ExpenseCount > 0
                    || SalesQuotationCount > 0 || SalesOrderCount > 0 || SalesProformaCount > 0 || DeliveryChallanCount > 0
                    || SalesCount > 0 || SalesReturnCount > 0 || CustomerPaymentCount > 0
                    //|| CustomerRefundCount > 0 
                    //|| BranchCount > 0 
                    || ContraCount > 0 || JournalCount > 0)
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
                ClsBranch oClsRole = new ClsBranch()
                {
                    BranchId = obj.BranchId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBranch.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.BranchId).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Branch",
                    CompanyId = obj.CompanyId,
                    Description = "Branch \"" + oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.BranchCode).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.BranchId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Branch deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        //public async Task<IHttpActionResult> ActiveBranchs(ClsBranchVm obj)
        //{
        //    var det = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true
        //    && a.CityId == obj.CityId).Select(a => new
        //    {
        //        BranchId = a.BranchId,
        //        BranchCode = a.BranchCode,
        //        Branch = a.Branch,
        //    }).OrderBy(a => a.Branch).ToList();
        //    data = new
        //    {
        //        Status = 1,
        //        Message = "found",
        //        Data = new
        //        {
        //            Branchs = det,
        //        }
        //    };
        //    return await Task.FromResult(Ok(data));
        //}

        public async Task<IHttpActionResult> ActiveBranchs(ClsBranchVm obj)
        {
            var Branchs = (from a in oConnectionContext.DbClsBranch
                           join b in oConnectionContext.DbClsUserBranchMap
                           on a.BranchId equals b.BranchId
                           where a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false
                           && b.UserId == obj.AddedBy && b.IsActive == true && b.IsDeleted == false
                           select new
                           {
                               a.BranchId,
                               a.Branch
                           }).OrderBy(a => a.Branch).ToList();

            //var Branchs = oConnectionContext.DbClsUserBranchMap.Where(b => b.UserId == obj.AddedBy && b.IsActive == true
            //      && b.IsDeleted == false && b.IsCancelled == false).Select(b => new 
            //      { b.BranchId, 
            //          Branch = oConnectionContext.DbClsBranch.Where(c => c.BranchId == b.BranchId).Select(c => c.Branch).FirstOrDefault() 
            //      }).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Branchs = Branchs,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BranchAdmin(ClsBranchVm obj)
        {
            obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

            var det = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => new
            {
                a.IsMain,
                BranchId = a.BranchId,
                BranchCode = a.BranchCode,
                Branch = a.Branch,
                ContactName = a.ContactName,
                Mobile = a.Mobile,
                a.AltMobileNo,
                Email = a.Email,
                CityId = a.CityId,
                StateId = a.StateId,
                //CountryId = a.CountryId,
                CountryId = oConnectionContext.DbClsBusinessSettings.Where(b => b.CompanyId == obj.Under).Select(b => b.CountryId).FirstOrDefault(),
                Address = a.Address,
                Zipcode = a.Zipcode,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                a.TaxId,
                a.TaxNo,
                a.EnableInlineTax,
                a.ContactPersonId
            }).FirstOrDefault();

            //var Countrys = oConnectionContext.DbClsCountry.Where(a => a.IsDeleted == false && a.IsCancelled == false && a.IsActive == true).Select(a => new
            //{
            //    CountryId = a.CountryId,
            //    a.CountryCode,
            //    Country = a.Country,
            //}).OrderBy(a => a.Country).ToList();

            var States = oConnectionContext.DbClsState.Where(a =>
            //a.CompanyId == obj.CompanyId && 
            a.IsDeleted == false && a.IsActive == true
            && a.CountryId == det.CountryId
            ).Select(a => new
            {
                StateId = a.StateId,
                a.StateCode,
                State = a.State,
            }).OrderBy(a => a.State).ToList();

            var Citys = oConnectionContext.DbClsCity.Where(a => a.CompanyId == obj.CompanyId && a.CountryId == det.CountryId
            && a.StateId == det.StateId && a.IsDeleted == false && a.IsActive == true).Select(a => new
            {
                CityId = a.CityId,
                a.CityCode,
                City = a.City,
            }).OrderBy(a => a.City).ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Branch = det,
                    //Countrys = Countrys,
                    States = States,
                    Citys = Citys,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateBranchAdmin(ClsBranchVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                obj.Under = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();

                obj.BranchId = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.Under && a.IsMain == true).Select(a => a.BranchId).FirstOrDefault();

                //obj.CountryId = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.CountryId).FirstOrDefault();

                //if (obj.Branch == null || obj.Branch == "")
                //{
                //    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                //    isError = true;
                //}
                if (obj.Mobile == "" || obj.Mobile == null)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divMobileNo" });
                    isError = true;
                }
                if (obj.Mobile != null && obj.Mobile != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.Mobile);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Contact No", Id = "divMobileNo" });
                        isError = true;
                    }
                }
                if (obj.AltMobileNo != null && obj.AltMobileNo != "")
                {
                    bool check = oCommonController.MobileValidationCheck(obj.AltMobileNo);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Alternative Contact No", Id = "divAltMobileNo" });
                        isError = true;
                    }
                }

                if (obj.Email != null && obj.Email != "")
                {
                    bool check = oCommonController.EmailValidationCheck(obj.Email);
                    if (check == false)
                    {
                        errors.Add(new ClsError { Message = "Invalid Email Id", Id = "divEmailId" });
                        isError = true;
                    }
                }
                if (obj.CountryId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCountry" });
                    isError = true;
                }
                if (obj.StateId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divState" });
                    isError = true;
                }
                if (obj.CityId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCity" });
                    isError = true;
                }
                if (obj.Zipcode == null || obj.Zipcode == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divZipcode" });
                    isError = true;
                }
                if (obj.Address == null || obj.Address == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAddress" });
                    isError = true;
                }

                //if (oConnectionContext.DbClsBranch.Where(a => a.Branch.ToLower() == obj.Branch.ToLower() && a.BranchId != obj.BranchId
                //&& a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsCancelled == false).Count() > 0)
                //{
                //    errors.Add(new ClsError { Message = "Duplicate Branch Name exists", Id = "divBranch" });
                //    isError = true;
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

                ClsBranch oBranch = new ClsBranch()
                {
                    BranchId = obj.BranchId,
                    //BranchCode = obj.BranchCode,
                    Branch = obj.Branch,
                    ContactName = obj.ContactName,
                    Mobile = obj.Mobile,
                    AltMobileNo = obj.AltMobileNo,
                    Email = obj.Email,
                    CityId = obj.CityId,
                    StateId = obj.StateId,
                    CountryId = obj.CountryId,
                    Address = obj.Address,
                    Zipcode = obj.Zipcode,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    ContactPersonId = obj.ContactPersonId,
                    TaxId = obj.TaxId,
                    TaxNo = obj.TaxNo,
                    EnableInlineTax = obj.EnableInlineTax
                };
                oConnectionContext.DbClsBranch.Attach(oBranch);
                oConnectionContext.Entry(oBranch).Property(x => x.BranchId).IsModified = true;
                //oConnectionContext.Entry(oBranch).Property(x => x.BranchCode).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Branch).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ContactName).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Mobile).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.AltMobileNo).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Email).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.CityId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.StateId).IsModified = true;
                //oConnectionContext.Entry(oBranch).Property(x => x.CountryId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Address).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.Zipcode).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.ContactPersonId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.TaxId).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.TaxNo).IsModified = true;
                oConnectionContext.Entry(oBranch).Property(x => x.EnableInlineTax).IsModified = true;
                oConnectionContext.SaveChanges();

                //ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                //{
                //    AddedBy = obj.AddedBy,
                //    Browser = obj.Browser,
                //    Category = "Branch",
                //    CompanyId = obj.CompanyId,
                //    Description = "modified " + obj.Branch,
                //    Id = oBranch.BranchId,
                //    IpAddress = obj.IpAddress,
                //    Platform = obj.Platform,
                //    Type = "Update"
                //};
                //oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Business address updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BranchCount(ClsBranchVm obj)
        {
            int BranchCount = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    TotalCount = BranchCount,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> MainBranch(ClsBranchVm obj)
        {
            var det = oConnectionContext.DbClsBranch.Where(a => a.IsMain == true && a.CompanyId == obj.CompanyId).Select(a => new
            {
                a.PrefixId,
                a.TaxSettingId,
                a.IsMain,
                BranchId = a.BranchId,
                BranchCode = a.BranchCode,
                Branch = a.Branch,
                ContactName = a.ContactName,
                Mobile = a.Mobile,
                a.AltMobileNo,
                Email = a.Email,
                CityId = a.CityId,
                StateId = a.StateId,
                CountryId = a.CountryId,
                Address = a.Address,
                Zipcode = a.Zipcode,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
                Country = oConnectionContext.DbClsCountry.Where(b => b.CountryId == a.CountryId).Select(b => b.Country).FirstOrDefault(),
                State = oConnectionContext.DbClsState.Where(b => b.StateId == a.StateId).Select(b => b.State).FirstOrDefault(),
                City = oConnectionContext.DbClsCity.Where(b => b.CityId == a.CityId).Select(b => b.City).FirstOrDefault(),
                PaymentTypes = oConnectionContext.DbClsPaymentType.Where(b => b.CompanyId == obj.CompanyId && b.IsActive == true
                && b.IsDeleted == false && b.IsAdvance == false).Select(b => new
                {
                    IsPosShown = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Select(c => c.IsPosShown).FirstOrDefault(),
                    AccountId = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
                    c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Select(c => c.AccountId).FirstOrDefault(),
                    b.PaymentType,
                    b.PaymentTypeId,
                    IsChecked = oConnectionContext.DbClsBranchPaymentTypeMap.Where(c => c.CompanyId == a.CompanyId &&
                    c.BranchId == a.BranchId && c.PaymentTypeId == b.PaymentTypeId && c.IsActive == true && c.IsDeleted == false).Count() == 0 ? false : true,
                }).ToList(),
                a.TaxId,
                a.TaxNo,
                a.EnableInlineTax,
                a.ContactPersonId
            }).FirstOrDefault();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Branch = det,
                }
            };
            return await Task.FromResult(Ok(data));
        }

    }
}
