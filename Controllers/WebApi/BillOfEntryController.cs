using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class BillOfEntryController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> BillOfEntry(ClsBillOfEntryVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            if (obj.BillOfEntryId == 0)
            {
                var det = (from bb in oConnectionContext.DbClsPurchase
                           where bb.PurchaseId == obj.PurchaseId && bb.CompanyId == obj.CompanyId && bb.IsActive == true && bb.IsDeleted == false && bb.IsCancelled == false
                           select new
                           {
                               bb.BranchId,
                               bb.SupplierId,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == bb.SupplierId).Select(e => e.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                               bb.PurchaseId,
                               PurchaseInvoiceNo = bb.ReferenceNo,
                               BillOfEntryDetails = (from b in oConnectionContext.DbClsPurchaseDetails
                                                      join c in oConnectionContext.DbClsItemDetails
                                      on b.ItemDetailsId equals c.ItemDetailsId
                                                      join d in oConnectionContext.DbClsItem
                                                      on c.ItemId equals d.ItemId
                                                      where b.PurchaseId == obj.PurchaseId && b.IsDeleted == false
                                                      select new
                                                      {
                                                          d.ItemId,
                                                          d.ProductType,
                                                          c.ItemDetailsId,
                                                          d.ItemName,
                                                          SKU = c.SKU == null ? d.SkuCode : c.SKU,
                                                          c.VariationDetailsId,
                                                          VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                          UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                                          b.PurchaseDetailsId,
                                                          AssessableValue = b.AmountExcTax,
                                                          AmountExcTax = b.AmountExcTax,
                                                      }).ToList(),
                           }).FirstOrDefault();
                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        BillOfEntry = det,
                    }
                };
            }
            else
            {
                var det = (from a in oConnectionContext.DbClsBillOfEntry
                           where a.BillOfEntryId == obj.BillOfEntryId && a.CompanyId == obj.CompanyId
                           select new
                           {
                               a.BranchId,
                               a.SupplierId,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(bb => bb.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.SupplierId).Select(c => c.CurrencyId).FirstOrDefault()).Select(bb => bb.CurrencySymbol).FirstOrDefault(),
                               a.PurchaseId,
                               PurchaseInvoiceNo = oConnectionContext.DbClsPurchase.Where(b => b.PurchaseId == a.PurchaseId).Select(b => b.ReferenceNo).FirstOrDefault(),
                               a.BillOfEntryNo,
                               a.BillOfEntryId,
                               a.PortCode,
                               a.BillOfEntryDate,
                               a.PaidThrough,
                               a.ReferenceNo,
                               a.Subtotal,
                               a.TotalCustomDuty,
                               a.TotalTaxAmount,
                               a.GrandTotal,
                               a.TotalAmountPaid,
                               a.AttachDocument,
                               BillOfEntryDetails = (from b in oConnectionContext.DbClsBillOfEntryDetails
                                                      join c in oConnectionContext.DbClsItemDetails
                                                      on b.ItemDetailsId equals c.ItemDetailsId
                                                      join d in oConnectionContext.DbClsItem
                                                                      on c.ItemId equals d.ItemId
                                                      where b.BillOfEntryId == a.BillOfEntryId && b.IsDeleted == false
                                                      select new
                                                      {
                                                          d.ItemId,
                                                          d.ProductType,
                                                          c.ItemDetailsId,
                                                          d.ItemName,
                                                          SKU = c.SKU == null ? d.SkuCode : c.SKU,
                                                          c.VariationDetailsId,
                                                          VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                          UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                                          b.PurchaseDetailsId,
                                                          b.AssessableValue,
                                                          b.CustomDuty,
                                                          b.AmountExcTax,
                                                          b.TaxId,
                                                          b.TaxAmount,
                                                          b.AmountIncTax,
                                                      }).ToList(),
                           }).FirstOrDefault();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        BillOfEntry = det,
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertBillOfEntry(ClsBillOfEntryVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            string PrefixType = "";
            long PrefixUserMapId = 0;

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.BillOfEntryNo == "" || obj.BillOfEntryNo == null)
                {
                    errors.Add(new ClsError { Message = "Bill Of Entry No is required", Id = "divBillOfEntryNo" });
                    isError = true;
                }

                if (obj.PortCode == "" || obj.PortCode == null)
                {
                    errors.Add(new ClsError { Message = "PortCode is required", Id = "divPortCode" });
                    isError = true;
                }

                if (obj.BillOfEntryDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "Bill Of Entry Date is required", Id = "divBillOfEntryDate" });
                    isError = true;
                }

                if (obj.PaidThrough == 0)
                {
                    errors.Add(new ClsError { Message = "Paid Through is required", Id = "divPaidThrough" });
                    isError = true;
                }

                if (obj.BillOfEntryNo != "" && obj.BillOfEntryNo != null)
                {
                    if (oConnectionContext.DbClsBillOfEntry.Where(a => a.BillOfEntryNo == obj.BillOfEntryNo
                    && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Bill Of Entry No is already used", Id = "divBillOfEntryNo" });
                        isError = true;
                    }
                }

                if (obj.BillOfEntryDetails != null)
                {
                    foreach (var item in obj.BillOfEntryDetails)
                    {
                        if (item.TaxId == 0)
                        {
                            errors.Add(new ClsError { Message = "Tax is required", Id = "divTax" + item.DivId });
                            isError = true;
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

                long PrefixId = 0;
                if (obj.ReferenceNo == "" || obj.ReferenceNo == null)
                {
                    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    PrefixType = "Bill Of Entry";
                    var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                          join b in oConnectionContext.DbClsPrefixUserMap
                                           on a.PrefixMasterId equals b.PrefixMasterId
                                          where a.IsActive == true && a.IsDeleted == false &&
                                          b.CompanyId == obj.CompanyId && b.IsActive == true
                                          && b.IsDeleted == false && a.PrefixType.ToLower() == PrefixType.ToLower()
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

                ClsBillOfEntry oClsBillOfEntry = new ClsBillOfEntry()
                {
                    SupplierId = obj.SupplierId,
                    BranchId = obj.BranchId,
                    PurchaseId = obj.PurchaseId,
                    BillOfEntryNo = obj.BillOfEntryNo,
                    PortCode = obj.PortCode,
                    BillOfEntryDate = obj.BillOfEntryDate.AddHours(5).AddMinutes(30),
                    PaidThrough = obj.PaidThrough,
                    Subtotal = obj.Subtotal,
                    TotalCustomDuty = obj.TotalCustomDuty,
                    TotalTaxAmount = obj.TotalTaxAmount,
                    GrandTotal = obj.GrandTotal,
                    TotalAmountPaid = obj.TotalAmountPaid,
                    CompanyId = obj.CompanyId,
                    IsActive = obj.IsActive,
                    IsDeleted = obj.IsDeleted,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ReferenceNo = obj.ReferenceNo
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/BillOfEntry/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BillOfEntry/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsBillOfEntry.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsBillOfEntry.Add(oClsBillOfEntry);
                oConnectionContext.SaveChanges();

                if (obj.BillOfEntryDetails != null)
                {
                    foreach (var BillOfEntry in obj.BillOfEntryDetails)
                    {
                        ClsBillOfEntryDetails oClsBillOfEntryDetails = new ClsBillOfEntryDetails()
                        {
                            BillOfEntryId = oClsBillOfEntry.BillOfEntryId,
                            PurchaseId = obj.PurchaseId,
                            PurchaseDetailsId = BillOfEntry.PurchaseDetailsId,
                            ItemId = BillOfEntry.ItemId,
                            ItemDetailsId = BillOfEntry.ItemDetailsId,
                            AssessableValue = BillOfEntry.AssessableValue,
                            CustomDuty = BillOfEntry.CustomDuty,
                            AmountExcTax = BillOfEntry.AmountExcTax,
                            TaxId = BillOfEntry.TaxId,
                            TaxAmount = BillOfEntry.TaxAmount,
                            AmountIncTax = BillOfEntry.AmountIncTax,
                            CompanyId = obj.CompanyId,
                            IsActive = BillOfEntry.IsActive,
                            IsDeleted = BillOfEntry.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsBillOfEntryDetails.Add(oClsBillOfEntryDetails);
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Bill Of Entry",
                    CompanyId = obj.CompanyId,
                    Description = "Bill Of Entry \"" + obj.BillOfEntryNo + "\" created",
                    Id = oClsBillOfEntry.BillOfEntryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Bill Of Entry created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateBillOfEntry(ClsBillOfEntryVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.BillOfEntryNo == "" || obj.BillOfEntryNo == null)
                {
                    errors.Add(new ClsError { Message = "Bill Of Entry No is required", Id = "divBillOfEntryNo" });
                    isError = true;
                }

                if (obj.PortCode == "" || obj.PortCode == null)
                {
                    errors.Add(new ClsError { Message = "PortCode is required", Id = "divPortCode" });
                    isError = true;
                }

                if (obj.BillOfEntryDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "Bill Of Entry Date is required", Id = "divBillOfEntryDate" });
                    isError = true;
                }

                if (obj.PaidThrough == 0)
                {
                    errors.Add(new ClsError { Message = "Paid Through is required", Id = "divPaidThrough" });
                    isError = true;
                }

                //if (obj.BillOfEntryNo != "" && obj.BillOfEntryNo != null)
                //{
                //    if (oConnectionContext.DbClsBillOfEntry.Where(a => a.BillOfEntryNo == obj.BillOfEntryNo
                //    && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Bill Of Entry No is already used", Id = "divBillOfEntryNo" });
                //        isError = true;
                //    }
                //}

                if (obj.BillOfEntryDetails != null)
                {
                    foreach (var item in obj.BillOfEntryDetails)
                    {
                        if (item.TaxId == 0)
                        {
                            errors.Add(new ClsError { Message = "Tax is required", Id = "divTax" + item.DivId });
                            isError = true;
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

                ClsBillOfEntry oClsBillOfEntry = new ClsBillOfEntry()
                {
                    BillOfEntryId = obj.BillOfEntryId,
                    SupplierId = obj.SupplierId,
                    BranchId = obj.BranchId,
                    PurchaseId = obj.PurchaseId,
                    BillOfEntryNo = obj.BillOfEntryNo,
                    PortCode = obj.PortCode,
                    BillOfEntryDate = obj.BillOfEntryDate.AddHours(5).AddMinutes(30),
                    PaidThrough = obj.PaidThrough,
                    Subtotal = obj.Subtotal,
                    TotalCustomDuty = obj.TotalCustomDuty,
                    TotalTaxAmount = obj.TotalTaxAmount,
                    GrandTotal = obj.GrandTotal,
                    TotalAmountPaid = obj.TotalAmountPaid,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };

                string pic1 = oConnectionContext.DbClsBillOfEntry.Where(a => a.BillOfEntryId == obj.BillOfEntryId).Select(a => a.AttachDocument).FirstOrDefault();
                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    if (pic1 != "" && pic1 != null)
                    {
                        if ((System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath(pic1))))
                        {
                            System.IO.File.Delete(System.Web.Hosting.HostingEnvironment.MapPath(pic1));
                        }
                    }

                    filepathPass = "/ExternalContents/Images/BillOfEntry/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/BillOfEntry/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsBillOfEntry.AttachDocument = filepathPass;
                }
                else
                {
                    oClsBillOfEntry.AttachDocument = pic1;
                }

                oConnectionContext.DbClsBillOfEntry.Attach(oClsBillOfEntry);
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.SupplierId).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.PurchaseId).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.BillOfEntryNo).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.PortCode).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.BillOfEntryDate).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.PaidThrough).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.TotalCustomDuty).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.TotalAmountPaid).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.BillOfEntryDetails != null)
                {
                    foreach (var BillOfEntry in obj.BillOfEntryDetails)
                    {
                        if (BillOfEntry.BillOfEntryDetailsId == 0)
                        {
                            ClsBillOfEntryDetails oClsBillOfEntryDetails = new ClsBillOfEntryDetails()
                            {
                                BillOfEntryId = oClsBillOfEntry.BillOfEntryId,
                                PurchaseId = obj.PurchaseId,
                                PurchaseDetailsId = BillOfEntry.PurchaseDetailsId,
                                ItemId = BillOfEntry.ItemId,
                                ItemDetailsId = BillOfEntry.ItemDetailsId,
                                AssessableValue = BillOfEntry.AssessableValue,
                                CustomDuty = BillOfEntry.CustomDuty,
                                AmountExcTax = BillOfEntry.AmountExcTax,
                                TaxId = BillOfEntry.TaxId,
                                TaxAmount = BillOfEntry.TaxAmount,
                                AmountIncTax = BillOfEntry.AmountIncTax,
                                CompanyId = obj.CompanyId,
                                IsActive = BillOfEntry.IsActive,
                                IsDeleted = BillOfEntry.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsBillOfEntryDetails.Add(oClsBillOfEntryDetails);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {

                            ClsBillOfEntryDetails oClsBillOfEntryDetails = new ClsBillOfEntryDetails()
                            {
                                BillOfEntryDetailsId = BillOfEntry.BillOfEntryDetailsId,
                                BillOfEntryId = oClsBillOfEntry.BillOfEntryId,
                                PurchaseId = obj.PurchaseId,
                                PurchaseDetailsId = BillOfEntry.PurchaseDetailsId,
                                ItemId = BillOfEntry.ItemId,
                                ItemDetailsId = BillOfEntry.ItemDetailsId,
                                AssessableValue = BillOfEntry.AssessableValue,
                                CustomDuty = BillOfEntry.CustomDuty,
                                AmountExcTax = BillOfEntry.AmountExcTax,
                                TaxId = BillOfEntry.TaxId,
                                TaxAmount = BillOfEntry.TaxAmount,
                                AmountIncTax = BillOfEntry.AmountIncTax,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsBillOfEntryDetails.Attach(oClsBillOfEntryDetails);
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.BillOfEntryId).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.PurchaseId).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.PurchaseDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.AssessableValue).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.CustomDuty).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsBillOfEntryDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Bill Of Entry",
                    CompanyId = obj.CompanyId,
                    Description = "Bill Of Entry \"" + obj.BillOfEntryNo + "\" updated",
                    Id = oClsBillOfEntry.BillOfEntryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Bill Of Entry updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> BillOfEntryDelete(ClsBillOfEntryVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsBillOfEntry oClsBillOfEntry = new ClsBillOfEntry()
                {
                    BillOfEntryId = obj.BillOfEntryId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsBillOfEntry.Attach(oClsBillOfEntry);
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.BillOfEntryId).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsBillOfEntry).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Bill Of Entry",
                    CompanyId = obj.CompanyId,
                    Description = "Bill Of Entry \"" + oConnectionContext.DbClsBillOfEntry.Where(a => a.BillOfEntryId== obj.BillOfEntryId).Select(a => a.BillOfEntryNo).FirstOrDefault() + "\" deleted",
                    Id = oClsBillOfEntry.BillOfEntryId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Bill Of Entry deleted successfully",
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
