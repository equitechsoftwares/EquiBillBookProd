using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
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
    public class CashRegisterController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        public async Task<IHttpActionResult> AllCashRegisters(ClsCashRegisterVm obj)
        {
            if (obj.PageSize == 0)
            {
                obj.PageSize = oConnectionContext.DbClsBusinessSettings.Where(a => a.CompanyId == obj.CompanyId).Select(a => a.DatatablePageEntries).FirstOrDefault();
            }

            int skip = obj.PageSize * (obj.PageIndex - 1);

            var det = oConnectionContext.DbClsCashRegister.Where(a => a.CompanyId == obj.CompanyId && a.IsDeleted == false).Select(a => new
            {
                CashRegisterId = a.CashRegisterId,
                a.BranchId,
                a.OpenTime,
                a.CloseTime,
                a.CashInHand,
                a.Status,
                a.ClosingNote,
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

            if (obj.FromDate != DateTime.MinValue && obj.ToDate != DateTime.MinValue)
            {
                det = det.Where(a => a.AddedOn.Date >= obj.FromDate.AddHours(5).AddMinutes(30) && a.AddedOn.Date <= obj.ToDate.AddHours(5).AddMinutes(30)).ToList();
            }

            if (obj.Status != 0)
            {
                det = det.Where(a => a.Status == obj.Status).Select(a => a).ToList();
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CashRegisters = det.OrderByDescending(a => a.CashRegisterId).Skip(skip).Take(obj.PageSize).ToList(),
                    TotalCount = det.Count(),
                    ActiveCount = det.Where(a => a.IsActive == true).Count(),
                    InactiveCount = det.Where(a => a.IsActive == false).Count(),
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CashRegister(ClsCashRegister obj)
        {
            var det = oConnectionContext.DbClsCashRegister.Where(a => a.CashRegisterId == obj.CashRegisterId && a.CompanyId == obj.CompanyId).Select(a => new
            {
                CashRegisterId = a.CashRegisterId,
                a.BranchId,
                a.OpenTime,
                a.CloseTime,
                a.CashInHand,
                a.Status,
                a.ClosingNote,
                IsActive = a.IsActive,
                IsDeleted = a.IsDeleted,
                AddedBy = a.AddedBy,
                AddedOn = a.AddedOn,
                ModifiedBy = a.ModifiedBy,
                ModifiedOn = a.ModifiedOn,
                CompanyId = a.CompanyId,
            }).FirstOrDefault();
            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CashRegister = det
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertCashRegister(ClsCashRegisterVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.BranchId == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divBranch" });
                    isError = true;
                }

                if (obj.CashInHand == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divCashInHand" });
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

                ClsCashRegister oCashRegister = new ClsCashRegister()
                {
                    BranchId = obj.BranchId,
                    OpenTime = CurrentDate,
                    CashInHand = obj.CashInHand,
                    Status = 1,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    CompanyId = obj.CompanyId,
                };
                oConnectionContext.DbClsCashRegister.Add(oCashRegister);
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "POS",
                    CompanyId = obj.CompanyId,
                    Description = "POS \"Cash Register\" opened",
                    Id = oCashRegister.CashRegisterId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Cash Register created successfully",
                    Data = new
                    {
                        CashRegister = oCashRegister
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CloseCashRegister(ClsCashRegisterVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                obj.CashRegisterId = oConnectionContext.DbClsCashRegister.Where(a => a.AddedBy == obj.AddedBy && a.Status == 1).Select(a => a.CashRegisterId).FirstOrDefault();

                ClsCashRegister oCashRegister = new ClsCashRegister()
                {
                    CashRegisterId = obj.CashRegisterId,
                    ClosingNote = obj.ClosingNote,
                    CloseTime = CurrentDate,
                    Status = 2,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsCashRegister.Attach(oCashRegister);
                oConnectionContext.Entry(oCashRegister).Property(x => x.CashRegisterId).IsModified = true;
                oConnectionContext.Entry(oCashRegister).Property(x => x.ClosingNote).IsModified = true;
                oConnectionContext.Entry(oCashRegister).Property(x => x.CloseTime).IsModified = true;
                oConnectionContext.Entry(oCashRegister).Property(x => x.Status).IsModified = true;
                oConnectionContext.Entry(oCashRegister).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oCashRegister).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "POS",
                    CompanyId = obj.CompanyId,
                    Description = "POS \"Cash Register\" closed",
                    Id = oCashRegister.CashRegisterId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Cash Register updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CashRegisterStatusCheck(ClsCashRegister obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var det = oConnectionContext.DbClsCashRegister.Where(a => a.AddedBy == obj.AddedBy).Select(a => new ClsCashRegisterVm
                {
                    BranchId = a.BranchId,
                    CashRegisterId = a.CashRegisterId,
                    Status = a.Status,
                    Branch = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.Branch).FirstOrDefault(),
                    Name = oConnectionContext.DbClsUser.Where(b => b.UserId == obj.AddedBy).Select(b => b.Name).FirstOrDefault(),
                    OpenTime = a.OpenTime,
                    StateId = oConnectionContext.DbClsBranch.Where(b => b.BranchId == a.BranchId).Select(b => b.StateId).FirstOrDefault(),
                }).OrderByDescending(a => a.CashRegisterId).FirstOrDefault();

                if (det == null)
                {
                    det = new ClsCashRegisterVm
                    {
                        Status = 2
                    };
                }
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        CashRegister = det
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CurrentRegister(ClsCashRegister obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            obj.CashRegisterId = oConnectionContext.DbClsCashRegister.Where(a => a.AddedBy == obj.AddedBy && a.Status == 1).Select(a => a.CashRegisterId).FirstOrDefault();

            var det = oConnectionContext.DbClsCashRegister.Where(a => a.CashRegisterId == obj.CashRegisterId).Select(a => new ClsCashRegisterVm
            {
                OpenTime = a.OpenTime,
                CloseTime = CurrentDate,
                OpeningBalance = a.CashInHand,
                CashRegisterId = a.CashRegisterId,
                Status = a.Status,
                BranchId= a.BranchId
            }).FirstOrDefault();

            var PaymentTypes =
                (oConnectionContext.DbClsPaymentType.Where(aa => aa.CompanyId == obj.CompanyId
                 && aa.IsAdvance == true).Select(aa => new ClsPaymentTypeVm
                 {
                     PaymentTypeId = aa.PaymentTypeId,
                     PaymentType = aa.PaymentType,
                     Amount = (from x in oConnectionContext.DbClsSales
                               join y in oConnectionContext.DbClsCustomerPayment
                     on x.SalesId equals y.SalesId
                               where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
                              //&& x.SalesDate >= a.OpenTime && x.SalesDate <= a.CloseTime
                              && x.CashRegisterId == obj.CashRegisterId
                              && x.CompanyId == obj.CompanyId
                              && x.BranchId == det.BranchId
                              && y.PaymentTypeId == aa.PaymentTypeId
                              && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                              && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                               select y.Amount).DefaultIfEmpty().Sum()
                 })).Union
                 (from c in oConnectionContext.DbClsBranchPaymentTypeMap
                           join b in oConnectionContext.DbClsPaymentType
    on c.PaymentTypeId equals b.PaymentTypeId
                           where
                           c.BranchId == det.BranchId &&
                           c.IsDeleted == false && c.IsActive == true &&
                            b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                           select new ClsPaymentTypeVm
                           {
                               PaymentTypeId = c.PaymentTypeId,
                               PaymentType = b.PaymentType,
                               Amount = (from x in oConnectionContext.DbClsSales
                                         join y in oConnectionContext.DbClsCustomerPayment
 on x.SalesId equals y.SalesId
                                         where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
                //&& x.SalesDate >= a.OpenTime && x.SalesDate <= a.CloseTime
                && x.CashRegisterId == obj.CashRegisterId
                && x.CompanyId == obj.CompanyId
                && x.BranchId == det.BranchId
                && y.PaymentTypeId == c.PaymentTypeId
                && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                         select y.Amount).DefaultIfEmpty().Sum()
                           }).Distinct().ToList();

            decimal TotalSales = oConnectionContext.DbClsSales.Where(x => x.SalesType == "Sales" && x.Status != "Draft"
            //&& x.SalesDate >= det.OpenTime && x.SalesDate <= det.CloseTime
            && x.CashRegisterId == obj.CashRegisterId
                     && x.CompanyId == obj.CompanyId && x.BranchId == det.BranchId
                     && x.AddedBy == obj.AddedBy && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();

            decimal TotalPayment = (from x in oConnectionContext.DbClsSales
                                    join y in oConnectionContext.DbClsCustomerPayment
on x.SalesId equals y.SalesId
                                    where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
           //&& x.SalesDate >= det.OpenTime && x.SalesDate <= det.CloseTime
           && x.CashRegisterId == obj.CashRegisterId
           && x.CompanyId == obj.CompanyId && x.BranchId == det.BranchId
           && x.AddedBy == obj.AddedBy && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
           && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                    select y.Amount).DefaultIfEmpty().Sum();

            decimal TotalChangeReturn = (from x in oConnectionContext.DbClsSales
                                    join y in oConnectionContext.DbClsCustomerPayment
on x.SalesId equals y.SalesId
                                    where x.SalesType == "Sales" && y.Type == "Change Return" && x.Status != "Draft"
           //&& x.SalesDate >= det.OpenTime && x.SalesDate <= det.CloseTime
           && x.CashRegisterId == obj.CashRegisterId
           && x.CompanyId == obj.CompanyId && x.BranchId == det.BranchId
           && x.AddedBy == obj.AddedBy && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
           && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                    select y.Amount).DefaultIfEmpty().Sum();

            decimal TotalCash = PaymentTypes.Where(a => a.PaymentType == "Cash").Select(a => a.Amount).DefaultIfEmpty().Sum();

            det.ClosingBalance = det.OpeningBalance + (TotalCash - TotalChangeReturn);

            decimal TotalCreditSales = TotalSales - (TotalPayment - TotalChangeReturn);

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CashRegister = det,
                    TotalSales = TotalSales,
                    TotalPayment = TotalPayment - TotalChangeReturn,
                    TotalCreditSales = TotalCreditSales,
                    TotalChangeReturn = TotalChangeReturn,
                    PaymentTypes = PaymentTypes
                }
            };
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> CashRegisterReport(ClsCashRegisterVm obj)
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
            int TotalCount = 0;

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            List<ClsCashRegisterVm> det;
            if (obj.BranchId == 0)
            {
                det = oConnectionContext.DbClsCashRegister.Where(a => 
                oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == a.BranchId)
            && a.IsDeleted == false && DbFunctions.TruncateTime(a.OpenTime) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.OpenTime) <= obj.ToDate).Select(a => new ClsCashRegisterVm
                {
                    BranchName = oConnectionContext.DbClsBranch.Where(cc => cc.BranchId == a.BranchId).Select(cc => cc.Branch).FirstOrDefault(),
                    AddedBy = a.AddedBy,
                    OpenTime = a.OpenTime,
                    CloseTime = a.CloseTime,
                    OpeningBalance = a.CashInHand,
                    ChangeReturn = (from x in oConnectionContext.DbClsSales
                                    join y in oConnectionContext.DbClsCustomerPayment
                                    on x.SalesId equals y.SalesId
                                    where x.SalesType == "Sales" && y.Type == "Change Return" && x.Status != "Draft"
                                    && x.CashRegisterId == a.CashRegisterId
                                    && x.CompanyId == obj.CompanyId
                                    && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                                        l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == x.BranchId)
                                    && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                                    && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                    select y.Amount).DefaultIfEmpty().Sum(),
                    CashRegisterId = a.CashRegisterId,
                    Status = a.Status,
                    BranchId = a.BranchId,
                    Name = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Name).FirstOrDefault(),
                    EmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.EmailId).FirstOrDefault(),
                    MobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.MobileNo).FirstOrDefault(),
                    PaymentTypes =
                        (oConnectionContext.DbClsPaymentType.Where(aa => aa.CompanyId == obj.CompanyId
                    && aa.IsAdvance == true
                    ).Select(aa => new ClsPaymentTypeVm
                    {
                        PaymentTypeId = aa.PaymentTypeId,
                        PaymentType = aa.PaymentType,
                        Amount = (from x in oConnectionContext.DbClsSales
                                  join y in oConnectionContext.DbClsCustomerPayment
                        on x.SalesId equals y.SalesId
                                  where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
                                 && x.CashRegisterId == a.CashRegisterId
                                 && x.CompanyId == obj.CompanyId
                                 && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == x.BranchId)
                                 && y.PaymentTypeId == aa.PaymentTypeId
                                 && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                                 && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                  select y.Amount).DefaultIfEmpty().Sum()
                    })).Union(from c in oConnectionContext.DbClsBranchPaymentTypeMap
                            join b in oConnectionContext.DbClsPaymentType
                            on c.PaymentTypeId equals b.PaymentTypeId
                            where
                            oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == c.BranchId) &&
                            c.IsDeleted == false && c.IsActive == true &&
                            b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                            select new ClsPaymentTypeVm
                            {
                                PaymentTypeId = c.PaymentTypeId,
                                PaymentType = b.PaymentType,
                                Amount = (from x in oConnectionContext.DbClsSales
                                        join y in oConnectionContext.DbClsCustomerPayment
                            on x.SalesId equals y.SalesId
                                        where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
                            && x.CashRegisterId == a.CashRegisterId
                            && x.CompanyId == obj.CompanyId
                            && oConnectionContext.DbClsUserBranchMap.Where(l => l.CompanyId == obj.CompanyId &&
                            l.UserId == obj.AddedBy && l.IsActive == true && l.IsDeleted == false).Any(l => l.BranchId == x.BranchId)
                            && y.PaymentTypeId == c.PaymentTypeId
                            && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                            && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                        select y.Amount).DefaultIfEmpty().Sum()
                            }).Distinct().ToList()
                }).ToList();
                TotalCount = det.Count();
            }
            else
            {
                det = oConnectionContext.DbClsCashRegister.Where(a => 
                //a.AddedBy == obj.AddedBy
            a.BranchId == obj.BranchId && a.IsDeleted == false && DbFunctions.TruncateTime(a.OpenTime) >= obj.FromDate &&
                DbFunctions.TruncateTime(a.OpenTime) <= obj.ToDate).Select(a => new ClsCashRegisterVm
                {
                    BranchName = oConnectionContext.DbClsBranch.Where(cc => cc.BranchId == a.BranchId).Select(cc => cc.Branch).FirstOrDefault(),
                    AddedBy = a.AddedBy,
                    OpenTime = a.OpenTime,
                    CloseTime = a.CloseTime,
                    OpeningBalance = a.CashInHand,
                    ChangeReturn = (from x in oConnectionContext.DbClsSales
                                    join y in oConnectionContext.DbClsCustomerPayment
                                    on x.SalesId equals y.SalesId
                                    where x.SalesType == "Sales" && y.Type == "Change Return" && x.Status != "Draft"
                                    && x.CashRegisterId == a.CashRegisterId
                                    && x.CompanyId == obj.CompanyId
                                    && x.BranchId == a.BranchId
                                    && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                                    && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                    select y.Amount).DefaultIfEmpty().Sum(),
                    //ChangeReturn = (from x in oConnectionContext.DbClsSales
                    //                join y in oConnectionContext.DbClsPayment
                    //      on x.SalesId equals y.Id
                    //                where x.SalesType == "Pos" && y.Type == "Sales Payment" && x.Status != "Draft"
                    //               //&& x.SalesDate >= a.OpenTime && x.SalesDate <= a.CloseTime
                    //               && x.CashRegisterId == a.CashRegisterId
                    //               && x.CompanyId == obj.CompanyId
                    //               && x.BranchId == a.BranchId
                    //               && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                    //               && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                    //               && y.Type == "Change Return"
                    //                select y.Amount).DefaultIfEmpty().Sum(),
                    CashRegisterId = a.CashRegisterId,
                    Status = a.Status,
                    BranchId = a.BranchId,
                    Name = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.Name).FirstOrDefault(),
                    EmailId = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.EmailId).FirstOrDefault(),
                    MobileNo = oConnectionContext.DbClsUser.Where(b => b.UserId == a.AddedBy).Select(b => b.MobileNo).FirstOrDefault(),
                    PaymentTypes = (oConnectionContext.DbClsPaymentType.Where(aa => aa.CompanyId == obj.CompanyId
                && aa.IsAdvance == true).Select(aa => new ClsPaymentTypeVm
                {
                    PaymentTypeId = aa.PaymentTypeId,
                    PaymentType = aa.PaymentType,
                    Amount = (from x in oConnectionContext.DbClsSales
                              join y in oConnectionContext.DbClsCustomerPayment
on x.SalesId equals y.SalesId
                              where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
     //&& x.SalesDate >= a.OpenTime && x.SalesDate <= a.CloseTime
     && x.CashRegisterId == a.CashRegisterId
     && x.CompanyId == obj.CompanyId
     && x.BranchId == a.BranchId
     && y.PaymentTypeId == aa.PaymentTypeId
     && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
     && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                              select y.Amount).DefaultIfEmpty().Sum()
                })).Union(from c in oConnectionContext.DbClsBranchPaymentTypeMap
                          join b in oConnectionContext.DbClsPaymentType
   on c.PaymentTypeId equals b.PaymentTypeId
                          where c.BranchId == a.BranchId && c.IsDeleted == false && c.IsActive == true &&
                           b.CompanyId == obj.CompanyId && b.IsDeleted == false && b.IsActive == true
                          select new ClsPaymentTypeVm
                          {
                              PaymentTypeId = c.PaymentTypeId,
                              PaymentType = b.PaymentType,
                              Amount = (from x in oConnectionContext.DbClsSales
                                        join y in oConnectionContext.DbClsCustomerPayment
on x.SalesId equals y.SalesId
                                        where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
               //&& x.SalesDate >= a.OpenTime && x.SalesDate <= a.CloseTime
               && x.CashRegisterId == a.CashRegisterId
               && x.CompanyId == obj.CompanyId && x.BranchId == a.BranchId
               && y.PaymentTypeId == c.PaymentTypeId
               && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
               && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                        select y.Amount).DefaultIfEmpty().Sum()
                          }).Distinct().ToList()
                }).ToList();
                TotalCount = det.Count();
            }

            det = det.OrderByDescending(a => a.CashRegisterId).Take(obj.PageSize).ToList();

            if (obj.UserId != 0)
            {
                det = det.Where(a => a.AddedBy == obj.AddedBy).ToList();
            }

            if (obj.Type == 1)
            {
                det = det.Where(a => a.CloseTime == null).ToList();
            }

            if (obj.Type == 2)
            {
                det = det.Where(a => a.CloseTime != null).ToList();
            }

            List<ClsPaymentTypeVm> oClsPaymentTypeVm = new List<ClsPaymentTypeVm>();
            foreach (var item in det)
            {
                // Calculate TotalSales for this cash register
                decimal TotalSales = oConnectionContext.DbClsSales.Where(x => x.SalesType == "Sales" && x.Status != "Draft"
                    && x.CashRegisterId == item.CashRegisterId
                    && x.CompanyId == obj.CompanyId && x.BranchId == item.BranchId
                    && x.AddedBy == item.AddedBy && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false).Select(a => a.GrandTotal).DefaultIfEmpty().Sum();

                // Calculate TotalPayment for this cash register
                decimal TotalPayment = (from x in oConnectionContext.DbClsSales
                                        join y in oConnectionContext.DbClsCustomerPayment
                                        on x.SalesId equals y.SalesId
                                        where x.SalesType == "Sales" && y.Type == "Sales Payment" && x.Status != "Draft"
                                        && x.CashRegisterId == item.CashRegisterId
                                        && x.CompanyId == obj.CompanyId && x.BranchId == item.BranchId
                                        && x.AddedBy == item.AddedBy && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                                        && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                        select y.Amount).DefaultIfEmpty().Sum();

                // Calculate TotalChangeReturn for this cash register
                decimal TotalChangeReturn = (from x in oConnectionContext.DbClsSales
                                            join y in oConnectionContext.DbClsCustomerPayment
                                            on x.SalesId equals y.SalesId
                                            where x.SalesType == "Sales" && y.Type == "Change Return" && x.Status != "Draft"
                                            && x.CashRegisterId == item.CashRegisterId
                                            && x.CompanyId == obj.CompanyId && x.BranchId == item.BranchId
                                            && x.AddedBy == item.AddedBy && x.IsActive == true && x.IsDeleted == false && x.IsCancelled == false
                                            && y.IsActive == true && y.IsDeleted == false && y.IsCancelled == false
                                            select y.Amount).DefaultIfEmpty().Sum();

                // Calculate TotalCash from payment types
                decimal TotalCash = item.PaymentTypes.Where(a => a.PaymentType == "Cash").Select(a => a.Amount).DefaultIfEmpty().Sum();

                // Calculate ClosingBalance
                item.ClosingBalance = item.OpeningBalance + (TotalCash - TotalChangeReturn);

                // Calculate TotalCreditSales - Updated to match CurrentRegister logic
                item.TotalCreditSales = TotalSales - (TotalPayment - TotalChangeReturn);

                // Update ChangeReturn to match CurrentRegister logic
                item.ChangeReturn = TotalChangeReturn;

                // Add TotalSales and TotalPayment to the item
                item.TotalSales = TotalSales;
                item.TotalPayment = TotalPayment - TotalChangeReturn;

                foreach (var inner in item.PaymentTypes)
                {
                    if (oClsPaymentTypeVm.Where(a => a.PaymentType == inner.PaymentType).Count() == 0)
                    {
                        oClsPaymentTypeVm.Add(new ClsPaymentTypeVm { PaymentType = inner.PaymentType, Amount = inner.Amount });
                    }
                    else
                    {
                        decimal prevAmount = oClsPaymentTypeVm.Where(a => a.PaymentType == inner.PaymentType).Select(a => a.Amount).FirstOrDefault();
                        foreach (var tom in oClsPaymentTypeVm.Where(w => w.PaymentType == inner.PaymentType))
                        {
                            tom.Amount = prevAmount + inner.Amount;
                        }
                    }
                }
            }

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    CashRegisters = det,
                    PaymentTypes = oClsPaymentTypeVm,
                    TotalCount = TotalCount,
                    FromDate = obj.FromDate,
                    ToDate = obj.ToDate,
                    PageSize = obj.PageSize
                }
            };
            return await Task.FromResult(Ok(data));
        }

    }
}
