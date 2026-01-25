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
    public class ShippingBillController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();//Your db context goes here
        CommonController oCommonController = new CommonController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public async Task<IHttpActionResult> ShippingBill(ClsShippingBillVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);

            //long ShippingBillId = oConnectionContext.DbClsShippingBill.Where(a => a.SalesId == obj.SalesId && a.CompanyId == obj.CompanyId && a.IsActive == true &&
            //a.IsDeleted == false && a.IsCancelled == false).Select(a => a.ShippingBillId).FirstOrDefault();

            if (obj.ShippingBillId == 0)
            {
                var det = (from bb in oConnectionContext.DbClsSales
                           where bb.SalesId == obj.SalesId && bb.CompanyId == obj.CompanyId && bb.IsActive == true && bb.IsDeleted == false && bb.IsCancelled == false 
                           select new
                           {
                               bb.BranchId,
                               bb.CustomerId,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(b => b.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == bb.CustomerId).Select(e => e.CurrencyId).FirstOrDefault()).Select(b => b.CurrencySymbol).FirstOrDefault(),
                               bb.SalesId,
                               SalesInvoiceNo = bb.InvoiceNo,
                               ShippingBillDetails = (from b in oConnectionContext.DbClsSalesDetails
                                                      join c in oConnectionContext.DbClsItemDetails
                                      on b.ItemDetailsId equals c.ItemDetailsId
                                                      join d in oConnectionContext.DbClsItem
                                                      on c.ItemId equals d.ItemId
                                                      where b.SalesId == obj.SalesId && b.IsDeleted == false 
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
                                                          b.SalesDetailsId,
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
                        ShippingBill = det,
                    }
                };
            }
            else
            {
                var det = (from a in oConnectionContext.DbClsShippingBill
                           where a.ShippingBillId == obj.ShippingBillId && a.CompanyId == obj.CompanyId
                           select new
                           {
                               a.ShippingBillId,
                               a.ExportWithLut,
                               a.BranchId,
                               a.CustomerId,
                               CurrencySymbol = oConnectionContext.DbClsCurrency.Where(bb => bb.CurrencyId == oConnectionContext.DbClsUser.Where(e => e.UserId == a.CustomerId).Select(c => c.CurrencyId).FirstOrDefault()).Select(bb => bb.CurrencySymbol).FirstOrDefault(),
                               a.SalesId,
                               SalesInvoiceNo = oConnectionContext.DbClsSales.Where(b => b.SalesId == a.SalesId).Select(b => b.InvoiceNo).FirstOrDefault(),
                               a.ShippingBillNo,
                               a.PortCode,
                               a.ShippingBillDate,
                               a.PaidThrough,
                               //a.ReferenceNo,
                               a.Subtotal,
                               a.TotalCustomDuty,
                               a.TotalTaxAmount,
                               a.GrandTotal,
                               a.TotalAmountPaid,
                               a.AttachDocument,
                               ShippingBillDetails = (from b in oConnectionContext.DbClsShippingBillDetails
                                                      join c in oConnectionContext.DbClsItemDetails
                                                      on b.ItemDetailsId equals c.ItemDetailsId
                                                      join d in oConnectionContext.DbClsItem
                                                                      on c.ItemId equals d.ItemId
                                                      where b.ShippingBillId == a.ShippingBillId && b.IsDeleted == false
                                                      select new
                                                      {
                                                          b.ShippingBillDetailsId,
                                                          d.ItemId,
                                                          d.ProductType,
                                                          c.ItemDetailsId,
                                                          d.ItemName,
                                                          SKU = c.SKU == null ? d.SkuCode : c.SKU,
                                                          c.VariationDetailsId,
                                                          VariationName = oConnectionContext.DbClsVariationDetails.Where(cc => cc.VariationDetailsId == c.VariationDetailsId).Select(cc => cc.VariationDetails).FirstOrDefault(),
                                                          UnitName = oConnectionContext.DbClsUnit.Where(cc => cc.UnitId == d.UnitId).Select(cc => cc.UnitName).FirstOrDefault(),
                                                          b.SalesDetailsId,
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
                        ShippingBill = det,
                    }
                };
            }

            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> InsertShippingBill(ClsShippingBillVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            string PrefixType = "";
            long PrefixUserMapId = 0;

            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.ShippingBillNo == "" || obj.ShippingBillNo == null)
                {
                    errors.Add(new ClsError { Message = "Shipping Bill No is required", Id = "divShippingBillNo" });
                    isError = true;
                }

                if (obj.PortCode == "" || obj.PortCode == null)
                {
                    errors.Add(new ClsError { Message = "PortCode is required", Id = "divPortCode" });
                    isError = true;
                }

                if (obj.ShippingBillDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "Shipping Bill Date is required", Id = "divShippingBillDate" });
                    isError = true;
                }

                if (obj.PaidThrough == 0)
                {
                    errors.Add(new ClsError { Message = "Paid Through is required", Id = "divPaidThrough" });
                    isError = true;
                }

                if (obj.ShippingBillNo != "" && obj.ShippingBillNo != null)
                {
                    if (oConnectionContext.DbClsShippingBill.Where(a => a.ShippingBillNo == obj.ShippingBillNo
                    && a.CompanyId == obj.CompanyId && a.IsDeleted == false).Count() > 0)
                    {
                        errors.Add(new ClsError { Message = "Shipping Bill No is already used", Id = "divShippingBillNo" });
                        isError = true;
                    }
                }

                if (obj.ShippingBillDetails != null)
                {
                    foreach (var item in obj.ShippingBillDetails)
                    {
                        if (item.TaxId == 0)
                        {
                            errors.Add(new ClsError { Message = "Tax is required", Id = "divTax" + item.DivId });
                            isError = true;
                        }
                    }
                }

                int PayTaxForExport = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.PayTaxForExport).FirstOrDefault();

                if(PayTaxForExport == 1)
                {
                    if(obj.ExportWithLut == 1)
                    {
                        errors.Add(new ClsError { Message = "LUT or Bond cannot be selected for the shipping bill, as the linked invoice does not specify this option", Id = "divExportWithLut" });
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

                long PrefixId = 0;
                if (obj.ShippingBillNo == "" || obj.ShippingBillNo == null)
                {
                    PrefixId = oConnectionContext.DbClsBranch.Where(a => a.BranchId == obj.BranchId).Select(a => a.PrefixId).FirstOrDefault();
                    PrefixType = "Shipping Bill";
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
                    obj.ShippingBillNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                }

                ClsShippingBill oClsShippingBill = new ClsShippingBill()
                {
                    CustomerId = obj.CustomerId,
                    BranchId = obj.BranchId,
                    SalesId = obj.SalesId,
                    ShippingBillNo = obj.ShippingBillNo,
                    PortCode = obj.PortCode,
                    ShippingBillDate = obj.ShippingBillDate.AddHours(5).AddMinutes(30),
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
                    //ReferenceNo = obj.ReferenceNo,
                    ExportWithLut = obj.ExportWithLut
                };

                if (obj.AttachDocument != "" && obj.AttachDocument != null)
                {
                    string filepathPass = "";

                    filepathPass = "/ExternalContents/Images/ShippingBill/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ShippingBill/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsShippingBill.AttachDocument = filepathPass;
                }

                oConnectionContext.DbClsShippingBill.Add(oClsShippingBill);
                oConnectionContext.SaveChanges();

                if (obj.ShippingBillDetails != null)
                {
                    foreach (var ShippingBill in obj.ShippingBillDetails)
                    {
                        ClsShippingBillDetails oClsShippingBillDetails = new ClsShippingBillDetails()
                        {
                            ShippingBillId = oClsShippingBill.ShippingBillId,
                            SalesId = obj.SalesId,
                            SalesDetailsId = ShippingBill.SalesDetailsId,
                            ItemId = ShippingBill.ItemId,
                            ItemDetailsId = ShippingBill.ItemDetailsId,
                            AssessableValue = ShippingBill.AssessableValue,
                            CustomDuty = ShippingBill.CustomDuty,
                            AmountExcTax = ShippingBill.AmountExcTax,
                            TaxId = ShippingBill.TaxId,
                            TaxAmount = ShippingBill.TaxAmount,
                            AmountIncTax = ShippingBill.AmountIncTax,
                            CompanyId = obj.CompanyId,
                            IsActive = ShippingBill.IsActive,
                            IsDeleted = ShippingBill.IsDeleted,
                            AddedBy = obj.AddedBy,
                            AddedOn = CurrentDate,
                        };

                        //ConnectionContext ocon = new ConnectionContext();
                        oConnectionContext.DbClsShippingBillDetails.Add(oClsShippingBillDetails);
                        oConnectionContext.SaveChanges();
                    }
                }

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Shipping Bill",
                    CompanyId = obj.CompanyId,
                    Description = "Shipping Bill \"" + obj.ShippingBillNo + "\" created",
                    Id = oClsShippingBill.ShippingBillId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Insert"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Shipping Bill created successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> UpdateShippingBill(ClsShippingBillVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                if (obj.ShippingBillNo == "" || obj.ShippingBillNo == null)
                {
                    errors.Add(new ClsError { Message = "Shipping Bill No is required", Id = "divShippingBillNo" });
                    isError = true;
                }

                if (obj.PortCode == "" || obj.PortCode == null)
                {
                    errors.Add(new ClsError { Message = "PortCode is required", Id = "divPortCode" });
                    isError = true;
                }

                if (obj.ShippingBillDate == DateTime.MinValue)
                {
                    errors.Add(new ClsError { Message = "Shipping Bill Date is required", Id = "divShippingBillDate" });
                    isError = true;
                }

                if (obj.PaidThrough == 0)
                {
                    errors.Add(new ClsError { Message = "Paid Through is required", Id = "divPaidThrough" });
                    isError = true;
                }

                //if (obj.ShippingBillNo != "" && obj.ShippingBillNo != null)
                //{
                //    if (oConnectionContext.DbClsShippingBill.Where(a => a.ShippingBillNo == obj.ShippingBillNo
                //    && a.CompanyId == obj.CompanyId && a.IsDeleted == false && a.ShippingBillId != obj.ShippingBillId).Count() > 0)
                //    {
                //        errors.Add(new ClsError { Message = "Shipping Bill No is already used", Id = "divShippingBillNo" });
                //        isError = true;
                //    }
                //}

                if (obj.ShippingBillDetails != null)
                {
                    foreach (var item in obj.ShippingBillDetails)
                    {
                        if (item.TaxId == 0)
                        {
                            errors.Add(new ClsError { Message = "Tax is required", Id = "divTax" + item.DivId });
                            isError = true;
                        }
                    }
                }

                int PayTaxForExport = oConnectionContext.DbClsSales.Where(a => a.SalesId == obj.SalesId).Select(a => a.PayTaxForExport).FirstOrDefault();

                if (PayTaxForExport == 1)
                {
                    if (obj.ExportWithLut == 1)
                    {
                        errors.Add(new ClsError { Message = "LUT or Bond cannot be selected for the shipping bill, as the linked invoice does not specify this option", Id = "divExportWithLut" });
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

                ClsShippingBill oClsShippingBill = new ClsShippingBill()
                {
                    ShippingBillId = obj.ShippingBillId,
                    CustomerId = obj.CustomerId,
                    BranchId = obj.BranchId,
                    SalesId = obj.SalesId,
                    //ShippingBillNo = obj.ShippingBillNo,
                    PortCode = obj.PortCode,
                    ShippingBillDate = obj.ShippingBillDate.AddHours(5).AddMinutes(30),
                    PaidThrough = obj.PaidThrough,
                    Subtotal = obj.Subtotal,
                    TotalCustomDuty = obj.TotalCustomDuty,
                    TotalTaxAmount = obj.TotalTaxAmount,
                    GrandTotal = obj.GrandTotal,
                    TotalAmountPaid = obj.TotalAmountPaid,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    ExportWithLut = obj.ExportWithLut
                };

                string pic1 = oConnectionContext.DbClsShippingBill.Where(a => a.ShippingBillId == obj.ShippingBillId).Select(a => a.AttachDocument).FirstOrDefault();
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

                    filepathPass = "/ExternalContents/Images/ShippingBill/AttachDocument/" + DateTime.Now.ToString("ddmmyyyyhhmmss") + obj.FileExtensionAttachDocument;

                    string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    byte[] imageCheque = Convert.FromBase64String(base64);
                    Stream strm = new MemoryStream(imageCheque);
                    var targetFile = System.Web.Hosting.HostingEnvironment.MapPath(filepathPass);

                    var folder = System.Web.Hosting.HostingEnvironment.MapPath("~/ExternalContents/Images/ShippingBill/AttachDocument");
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }

                    //string base64 = obj.AttachDocument.Replace(obj.AttachDocument.Substring(0, obj.AttachDocument.IndexOf(',') + 1), "");
                    //File.WriteAllBytes(System.Web.Hosting.HostingEnvironment.MapPath(filepathPass), Convert.FromBase64String(base64));

                    oCommonController.GenerateThumbnails(imageCheque.Length, strm, targetFile);

                    oClsShippingBill.AttachDocument = filepathPass;
                }
                else
                {
                    oClsShippingBill.AttachDocument = pic1;
                }

                oConnectionContext.DbClsShippingBill.Attach(oClsShippingBill);
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.CustomerId).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.SalesId).IsModified = true;
                //oConnectionContext.Entry(oClsShippingBill).Property(x => x.ShippingBillNo).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.PortCode).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ShippingBillDate).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.PaidThrough).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.Subtotal).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.TotalCustomDuty).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.TotalTaxAmount).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.GrandTotal).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.TotalAmountPaid).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ExportWithLut).IsModified = true;
                oConnectionContext.SaveChanges();

                if (obj.ShippingBillDetails != null)
                {
                    foreach (var ShippingBill in obj.ShippingBillDetails)
                    {
                        if (ShippingBill.ShippingBillDetailsId == 0)
                        {
                            ClsShippingBillDetails oClsShippingBillDetails = new ClsShippingBillDetails()
                            {
                                ShippingBillId = oClsShippingBill.ShippingBillId,
                                SalesId = obj.SalesId,
                                SalesDetailsId = ShippingBill.SalesDetailsId,
                                ItemId = ShippingBill.ItemId,
                                ItemDetailsId = ShippingBill.ItemDetailsId,
                                AssessableValue = ShippingBill.AssessableValue,
                                CustomDuty = ShippingBill.CustomDuty,
                                AmountExcTax = ShippingBill.AmountExcTax,
                                TaxId = ShippingBill.TaxId,
                                TaxAmount = ShippingBill.TaxAmount,
                                AmountIncTax = ShippingBill.AmountIncTax,
                                CompanyId = obj.CompanyId,
                                IsActive = ShippingBill.IsActive,
                                IsDeleted = ShippingBill.IsDeleted,
                                AddedBy = obj.AddedBy,
                                AddedOn = CurrentDate,
                            };

                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsShippingBillDetails.Add(oClsShippingBillDetails);
                            oConnectionContext.SaveChanges();
                        }
                        else
                        {

                            ClsShippingBillDetails oClsShippingBillDetails = new ClsShippingBillDetails()
                            {
                                ShippingBillDetailsId = ShippingBill.ShippingBillDetailsId,
                                ShippingBillId = oClsShippingBill.ShippingBillId,
                                SalesId = obj.SalesId,
                                SalesDetailsId = ShippingBill.SalesDetailsId,
                                ItemId = ShippingBill.ItemId,
                                ItemDetailsId = ShippingBill.ItemDetailsId,
                                AssessableValue = ShippingBill.AssessableValue,
                                CustomDuty = ShippingBill.CustomDuty,
                                AmountExcTax = ShippingBill.AmountExcTax,
                                TaxId = ShippingBill.TaxId,
                                TaxAmount = ShippingBill.TaxAmount,
                                AmountIncTax = ShippingBill.AmountIncTax,
                                ModifiedBy = obj.AddedBy,
                                ModifiedOn = CurrentDate,
                            };
                            //ConnectionContext ocon = new ConnectionContext();
                            oConnectionContext.DbClsShippingBillDetails.Attach(oClsShippingBillDetails);
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.ShippingBillId).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.SalesId).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.SalesDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.ItemId).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.ItemDetailsId).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.AssessableValue).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.CustomDuty).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.AmountExcTax).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.TaxId).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.TaxAmount).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.AmountIncTax).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.ModifiedBy).IsModified = true;
                            oConnectionContext.Entry(oClsShippingBillDetails).Property(x => x.ModifiedOn).IsModified = true;
                            oConnectionContext.SaveChanges();
                        }
                    }
                }
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Shipping Bill",
                    CompanyId = obj.CompanyId,
                    Description = "Shipping Bill \"" + obj.ShippingBillNo + "\" updated",
                    Id = oClsShippingBill.ShippingBillId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Update"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Shipping Bill updated successfully",
                    Data = new
                    {
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        public async Task<IHttpActionResult> ShippingBillDelete(ClsShippingBillVm obj)
        {
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
                ClsShippingBill oClsShippingBill = new ClsShippingBill()
                {
                    ShippingBillId = obj.ShippingBillId,
                    IsDeleted = true,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                };
                oConnectionContext.DbClsShippingBill.Attach(oClsShippingBill);
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ShippingBillId).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.IsDeleted).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ModifiedBy).IsModified = true;
                oConnectionContext.Entry(oClsShippingBill).Property(x => x.ModifiedOn).IsModified = true;
                oConnectionContext.SaveChanges();

                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Shipping Bill",
                    CompanyId = obj.CompanyId,
                    Description = "Shipping Bill \"" + oConnectionContext.DbClsShippingBill.Where(a => a.ShippingBillId== obj.ShippingBillId).Select(a => a.ShippingBillNo).FirstOrDefault() + "\" deleted",
                    Id = oClsShippingBill.ShippingBillId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Shipping Bill deleted successfully",
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
