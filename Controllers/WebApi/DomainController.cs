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
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class DomainController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();

        public async Task<IHttpActionResult> AllDomains(ClsDomainVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                DomainId = a.DomainId,
                a.Domain,
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

            if (obj.Search != "" && obj.Search != null)
            {
                det = det.Where(a => a.Domain.ToLower().Contains(obj.Search.ToLower())).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Domains = det.OrderByDescending(a => a.DomainId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> Domain(ClsDomain obj)
        {
            var det = oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                DomainId = a.DomainId,
                a.Domain,
                a.Status,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Domain = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertDomain(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                if (obj.DomainId == 0)
                {
                    int TotalDomainUsed = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count();
                    int TotalDomain = oCommonController.fetchPlanQuantity(obj.CompanyId, "Domain");
                    if (TotalDomainUsed >= TotalDomain)
                    {
                        data = new
                        {
                            Status = 0,
                            Message = "Domain quota already used. Please upgrade addons from My Plan Menu",
                            Data = new
                            {
                            }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Domain == null || obj.Domain == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDomain" });
                    isError = true;
                }

                if (obj.Domain != null && obj.Domain != "")
                {
                    //if (oCommonController.GetIPAddress(obj.Domain) == "")
                    //{
                    //    errors.Add(new ClsError { Message = "Invalid domain", Id = "divDomain" });
                    //    isError = true;
                    //}
                    if (oConnectionContext.DbClsDomain.Where(a => a.Domain.ToLower() == obj.Domain.ToLower() && a.IsDeleted == false && a.DomainId != obj.DomainId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Domain name is already taken", Id = "divDomain" });
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

                if (obj.DomainId == 0)
                {
                    ClsDomain oClsDomain = new ClsDomain()
                    {
                        Domain = obj.Domain,
                        Status = 1,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.CompanyId,
                    };

                    oConnectionContext.DbClsDomain.Add(oClsDomain);
                    oConnectionContext.SaveChanges();

                    obj.DomainId = oClsDomain.DomainId;

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Domain",
                        CompanyId = obj.CompanyId,
                        Description = "Domain \"" + obj.Domain + "\" created",
                        Id = obj.DomainId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }
                else
                {
                    string query = "update \"tblDomain\" set \"Domain\"='" + obj.Domain + "' where \"DomainId\"=" + obj.DomainId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);

                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Domain",
                        CompanyId = obj.CompanyId,
                        Description = "Domain \"" + obj.Domain + "\" updated",
                        Id = obj.DomainId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Update"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }

                data = new
                {
                    Status = 1,
                    Message = "Domain created successfully",
                    Data = new
                    {
                        Domain = obj
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> VerifyDomainConnection(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                string ip = oCommonController.GetIPAddress(obj.Domain);

                if (ip != "108.181.191.157")
                {
                    errors.Add(new ClsError { Message = ip, Id = "divErrorMsg" });
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

                //long DomainId = oConnectionContext.DbClsDomain.Where(a => a.Domain.ToLower() == obj.Domain.ToLower() && a.IsActive == true &&
                //a.IsDeleted == false && a.IsCancelled == false).Select(a=>a.DomainId).FirstOrDefault();

                string query = "update \"tblDomain\" set \"Status\"=2 where \"DomainId\"=" + obj.DomainId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                query = "update \"tblUser\" set \"WhitelabelType\"=2 where \"UserId\"=" + obj.CompanyId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                //App setup
                ClsPwaSettings oClsPwaSettings = new ClsPwaSettings()
                {
                    DomainId = obj.DomainId,
                    BackgroundColor = "#fdfdfd",
                    ThemeColor = "#db4938",
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyId = obj.AddedBy
                };
                oConnectionContext.DbClsPwaSettings.Add(oClsPwaSettings);
                oConnectionContext.SaveChanges();
                //App setup

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Domain",
                    CompanyId = obj.CompanyId,
                    Description = "Domain \"" + oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId).Select(a => a.Domain).FirstOrDefault() + "\" verified",
                    Id = obj.DomainId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Domain verified successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DomainActiveInactive(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsDomain oClsDomain = new ClsDomain()
                {
                    DomainId = obj.DomainId,
                    IsActive = obj.IsActive,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsDomain.Attach(oClsDomain);
                oConnectionContext.Entry(oClsDomain).Property(x => x.DomainId).IsModified = true;
                oConnectionContext.Entry(oClsDomain).Property(x => x.IsActive).IsModified = true;
                oConnectionContext.Entry(oClsDomain).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsDomain).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Business Settings",
                    CompanyId = obj.CompanyId,
                    Description = "Business Settings \"" + oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId).Select(a => a.Domain).FirstOrDefault() + (obj.IsActive == true ? "\" activated " : "\" deactivated "),
                    Id = oClsDomain.DomainId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Domain " + (obj.IsActive == true ? "activated" : "deactivated") + " successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DomainDelete(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int Status = 1;
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsDomain oClsRole = new ClsDomain()
                {
                    DomainId = obj.DomainId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsDomain.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                if (oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.IsActive == true && a.Status == 2).Count() == 0)
                {
                    string query = "update \"tblUser\" set \"WhitelabelType\"=0 where \"UserId\"=" + obj.CompanyId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                if (obj.Domain.ToLower() == oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId).Select(a => a.Domain.ToLower()).FirstOrDefault())
                {
                    var ActiveLogins = oConnectionContext.DbClsLoginDetails.Where(a => a.IsLoggedOut == false &&
                    oConnectionContext.DbClsUser.Where(b => b.CompanyId == obj.CompanyId).Any(b => b.UserId == a.AddedBy)).Select(a => new
                    {
                        a.LoginDetailsId
                    }).ToList();

                    foreach (var item in ActiveLogins)
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

                    Status = 3;
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Domain",
                    CompanyId = obj.CompanyId,
                    Description = "Domain \"" + oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId).Select(a => a.Domain).FirstOrDefault() + "\" deleted",
                    Id = oClsRole.DomainId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = Status,
                    Message = "Domain deleted successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> DomainCheckForRedirection(ClsDomainVm obj)
        {
            long CompanyId = oConnectionContext.DbClsDomain.Where(a => a.Domain == obj.Domain && a.IsDeleted == false && a.IsActive == true).Select(a => a.CompanyId).FirstOrDefault();
            int WhitelabelType = oConnectionContext.DbClsUser.Where(a => a.UserId == CompanyId).Select(a => a.WhitelabelType).FirstOrDefault();

            string UserType = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => a.UserType).FirstOrDefault();

            var BusinessSetting = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == CompanyId).Select(a => new
            {
                a.ResellerTermsUrl,
                a.BusinessName,
                a.WebsiteUrl,
                a.BusinessLogo,
                a.ParentWebsiteUrl,
                a.Favicon
            }).FirstOrDefault();

            ClsUserVm oClsUser = new ClsUserVm();
            if (obj.AddedBy != 0)
            {
                oClsUser = oConnectionContext.DbClsUser.Where(a => a.UserId == obj.AddedBy).Select(a => new ClsUserVm
                {
                    Name = a.Name,
                    ProfilePic = a.ProfilePic == null ? "data:image/png;base64,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" : a.ProfilePic,
                }).FirstOrDefault();
            }

            data = new
            {
                Status = 1,
                Message = "",
                WhitelabelType = WhitelabelType,
                UserType = UserType,
                Data = new
                {
                    CompanyId = CompanyId,
                    BusinessSetting = BusinessSetting,
                    User = oClsUser,
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> AllDomainsAdmin(ClsDomainVm obj)
        {
            var det = oConnectionContext.DbClsDomain.Where(a => a.CompanyId == obj.UserId && a.IsDeleted == false).Select(a => new
            {
                DomainId = a.DomainId,
                a.Domain,
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

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Domains = det.OrderByDescending(a => a.DomainId).ToList(),
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DomainAdmin(ClsDomainVm obj)
        {
            var det = oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId && a.CompanyId == obj.UserId).Select(a => new
            {
                DomainId = a.DomainId,
                a.Domain,
                a.Status,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    Domain = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertDomainAdmin(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                if (obj.Domain == null || obj.Domain == "")
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divDomain" });
                    isError = true;
                }

                if (obj.Domain != null && obj.Domain != "")
                {
                    if (oCommonController.GetIPAddress(obj.Domain) == "")
                    {
                        errors.Add(new ClsError { Message = "Invalid domain", Id = "divDomain" });
                        isError = true;
                    }
                    if (oConnectionContext.DbClsDomain.Where(a => a.Domain.ToLower() == obj.Domain.ToLower() && a.IsDeleted == false && a.DomainId != obj.DomainId).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Domain name is already taken", Id = "divDomain" });
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

                if (obj.DomainId == 0)
                {
                    ClsDomain oClsDomain = new ClsDomain()
                    {
                        Domain = obj.Domain,
                        Status = 1,
                        IsActive = obj.IsActive,
                        IsDeleted = obj.IsDeleted,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        CompanyId = obj.UserId,
                    };

                    oConnectionContext.DbClsDomain.Add(oClsDomain);
                    oConnectionContext.SaveChanges();

                    obj.DomainId = oClsDomain.DomainId;
                }
                else
                {
                    string query = "update \"tblDomain\" set \"Domain\"='" + obj.Domain + "' where \"DomainId\"=" + obj.DomainId;
                    oConnectionContext.Database.ExecuteSqlCommand(query);
                }

                data = new
                {
                    Status = 1,
                    Message = "Domain created successfully",
                    Data = new
                    {
                        Domain = obj
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> VerifyDomainConnectionAdmin(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                bool isError = false;
                List<ClsError> errors = new List<ClsError>();

                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

                string ip = oCommonController.GetIPAddress(obj.Domain);

                if (ip != "108.181.191.157")
                {
                    errors.Add(new ClsError { Message = ip, Id = "divErrorMsg" });
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

                string query = "update \"tblDomain\" set \"Status\"=2 where \"DomainId\"=" + obj.DomainId;
                oConnectionContext.Database.ExecuteSqlCommand(query);

                long CompanyId = oConnectionContext.DbClsDomain.Where(a => a.DomainId == obj.DomainId).Select(a => a.CompanyId).FirstOrDefault();

                //App setup
                ClsPwaSettings oClsPwaSettings = new ClsPwaSettings()
                {
                    DomainId = obj.DomainId,
                    BackgroundColor = "#fdfdfd",
                    ThemeColor = "#db4938",
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    IsActive = true,
                    IsDeleted = false,
                    CompanyId = CompanyId
                };
                oConnectionContext.DbClsPwaSettings.Add(oClsPwaSettings);
                oConnectionContext.SaveChanges();
                //App setup

                data = new
                {
                    Status = 1,
                    Message = "Domain created successfully",
                    Data = new
                    {

                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> DomainDeleteAdmin(ClsDomainVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                int Status = 1;
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsDomain oClsRole = new ClsDomain()
                {
                    DomainId = obj.DomainId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsDomain.Attach(oClsRole);
                oConnectionContext.Entry(oClsRole).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsRole).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                data = new
                {
                    Status = Status,
                    Message = "Domain deleted successfully",
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
