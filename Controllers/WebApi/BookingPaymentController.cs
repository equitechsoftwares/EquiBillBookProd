using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace EquiBillBook.Controllers.WebApi
{
    [ExceptionHandlerAttribute]
    [IdentityBasicAuthenticationAttribute]
    public class BookingPaymentController : ApiController
    {
        dynamic data = null;
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();
        NotificationTemplatesController oNotificationTemplatesController = new NotificationTemplatesController();
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        [HttpPost]
        public async Task<IHttpActionResult> Insert(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Get booking details
                var booking = oConnectionContext.DbClsTableBooking
                    .Where(b => b.BookingId == obj.BookingId && b.CompanyId == obj.CompanyId)
                    .Select(b => new
                    {
                        b.BookingId,
                        b.CustomerId,
                        b.BranchId,
                        b.DepositAmount,
                        b.BookingNo
                    })
                    .FirstOrDefault();

                if (booking == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Booking not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Validate PaymentDate for credit application
                if (obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count > 0)
                {
                    if (obj.PaymentDate == DateTime.MinValue)
                    {
                        data = new
                        {
                            Status = 2,
                            Message = "",
                            Errors = new[] { new ClsError { Message = "This field is required", Id = "divCreditsAppliedDate" } },
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // Calculate total paid amount from centralized customer payments
                decimal totalPaid = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == obj.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Payment")
                    .Select(cp => cp.Amount)
                    .DefaultIfEmpty(0)
                    .Sum();

                // Handle credit application if CustomerPaymentIds are provided
                if (obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count > 0)
                {
                    // Validate total credits to apply
                    decimal totalCreditsToApply = obj.CustomerPaymentIds.Sum(a => a.Amount);
                    decimal dueAmount = booking.DepositAmount - totalPaid;
                    if (dueAmount < 0) dueAmount = 0;

                    if (totalCreditsToApply > dueAmount)
                    {
                        data = new
                        {
                            Status = 2,
                            Message = "Total credits to apply cannot exceed the due amount",
                            Errors = new[] { new ClsError { Message = "Total credits to apply cannot exceed the due amount", Id = "divCredits" } },
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    // Set payment type to Advance and amount to total credits
                    obj.PaymentType = "Advance";
                    obj.Amount = totalCreditsToApply;
                    
                    // Get advance payment type ID
                    obj.PaymentTypeId = oConnectionContext.DbClsPaymentType
                        .Where(a => a.CompanyId == obj.CompanyId && a.IsAdvance == true)
                        .Select(a => a.PaymentTypeId)
                        .FirstOrDefault();

                    // Update advance balances and create child payment records
                    foreach (var creditItem in obj.CustomerPaymentIds)
                    {
                        if (creditItem.Amount > 0)
                        {
                            // Update the parent credit payment
                            var parentPayment = oConnectionContext.DbClsCustomerPayment
                                .Where(cp => cp.CustomerPaymentId == creditItem.CustomerPaymentId)
                                .FirstOrDefault();

                            if (parentPayment != null && parentPayment.AmountRemaining >= creditItem.Amount)
                            {
                                // Update amount remaining
                                string updateQuery = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"-" + creditItem.Amount + 
                                    ",\"AmountUsed\"=\"AmountUsed\"+" + creditItem.Amount + " where \"CustomerPaymentId\"=" + creditItem.CustomerPaymentId;
                                oConnectionContext.Database.ExecuteSqlCommand(updateQuery);

                                // Create child payment record linking credit to booking
                                ClsCustomerPayment oChildPayment = new ClsCustomerPayment()
                                {
                                    CustomerId = booking.CustomerId,
                                    BookingId = booking.BookingId,
                                    SalesId = 0,
                                    SalesReturnId = 0,
                                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                                    PaymentTypeId = obj.PaymentTypeId,
                                    Amount = creditItem.Amount,
                                    Notes = "Credit applied from " + parentPayment.ReferenceNo + " to booking " + booking.BookingNo,
                                    CompanyId = obj.CompanyId,
                                    IsActive = true,
                                    IsDeleted = false,
                                    AddedBy = obj.AddedBy,
                                    AddedOn = CurrentDate,
                                    ModifiedBy = obj.AddedBy,
                                    ModifiedOn = CurrentDate,
                                    AttachDocument = null,
                                    Type = "Booking Deposit Payment",
                                    AccountId = oConnectionContext.DbClsAccount
                                        .Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.Type == "Deferred Income")
                                        .Select(a => a.AccountId)
                                        .FirstOrDefault(),
                                    BranchId = booking.BranchId,
                                    ReferenceNo = obj.ReferenceNo,
                                    IsDebit = 2,
                                    ParentId = creditItem.CustomerPaymentId,
                                    ReferenceId = oCommonController.CreateToken(),
                                    JournalAccountId = oConnectionContext.DbClsAccount
                                        .Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.Type == "Accounts Receivable")
                                        .Select(a => a.AccountId)
                                        .FirstOrDefault(),
                                    AmountRemaining = 0,
                                    IsDirectPayment = false,
                                    AmountUsed = 0,
                                    PaymentLinkId = 0,
                                    PlaceOfSupplyId = 0,
                                    TaxId = 0,
                                    IsBusinessRegistered = 0,
                                    GstTreatment = null,
                                    BusinessRegistrationNameId = 0,
                                    BusinessRegistrationNo = null,
                                    BusinessLegalName = null,
                                    BusinessTradeName = null,
                                    PanNo = null,
                                    TaxAccountId = 0,
                                    AmountExcTax = creditItem.Amount,
                                    TaxAmount = 0,
                                    IsCancelled = false,
                                    PrefixId = 0,
                                    IsAdvance = false
                                };
                                oConnectionContext.DbClsCustomerPayment.Add(oChildPayment);
                                oConnectionContext.SaveChanges();
                            }
                        }
                    }

                    // Update customer advance balance
                    string balanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"-" + totalCreditsToApply + " where \"UserId\"=" + booking.CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(balanceQuery);

                    // Recalculate total paid after credit application
                    totalPaid = oConnectionContext.DbClsCustomerPayment
                        .Where(cp => cp.BookingId == obj.BookingId &&
                                     cp.IsDeleted == false &&
                                     cp.IsCancelled == false &&
                                     cp.Type == "Booking Deposit Payment")
                        .Select(cp => cp.Amount)
                        .DefaultIfEmpty(0)
                        .Sum();
                    
                    // For credit applications, validation already happened above (lines 72-82)
                    // Skip the deposit amount check since credits are already validated
                }
                else
                {
                    // Check if this payment would exceed deposit amount (only for regular payments, not credits)
                    if (totalPaid + obj.Amount > booking.DepositAmount)
                    {
                        data = new
                        {
                            Status = 2,
                            Message = "Payment amount exceeds the deposit amount",
                            Errors = new[] { new ClsError { Message = "Payment amount exceeds the deposit amount", Id = "divAmount" } },
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }
                }

                // Only create a new payment record if this is not a credit application
                // (credit applications already created child records above)
                ClsCustomerPayment oCentralPayment = null;
                if (obj.CustomerPaymentIds == null || obj.CustomerPaymentIds.Count == 0)
                {
                    // Insert centralized record into tblCustomerPayment so that
                    // all customer-level payments (sales + bookings + bookings) are tracked together.
                    // We identify booking deposits by Type + BookingId.
                    string referenceId = oCommonController.CreateToken();

                    // Get Deferred Income account for JournalAccountId (used for unused credits)
                    long journalAccountId = obj.JournalAccountId > 0 
                        ? obj.JournalAccountId 
                        : oConnectionContext.DbClsAccount
                            .Where(a => a.CompanyId == obj.CompanyId && a.IsActive == true && a.IsDeleted == false && a.Type == "Deferred Income")
                            .Select(a => a.AccountId)
                            .FirstOrDefault();

                    oCentralPayment = new ClsCustomerPayment()
                {
                    CustomerId = booking.CustomerId,
                    BookingId = booking.BookingId,
                    SalesId = 0,
                    SalesReturnId = 0,
                    PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                    PaymentTypeId = obj.PaymentTypeId,
                    Amount = obj.Amount,
                    Notes = string.IsNullOrEmpty(obj.Notes)
                        ? "Booking Deposit Payment for " + booking.BookingNo
                        : obj.Notes,
                    CompanyId = obj.CompanyId,
                    IsActive = true,
                    IsDeleted = false,
                    AddedBy = obj.AddedBy,
                    AddedOn = CurrentDate,
                    ModifiedBy = obj.AddedBy,
                    ModifiedOn = CurrentDate,
                    AttachDocument = obj.AttachDocument,
                    Type = "Booking Deposit Payment",
                    AccountId = obj.AccountId,
                    BranchId = booking.BranchId,
                    ReferenceNo = obj.ReferenceNo,
                    IsDebit = 2, // same as sales receipts
                    ParentId = 0,
                    ReferenceId = referenceId,
                    JournalAccountId = journalAccountId,
                    AmountRemaining = obj.Amount,  // Store as unused credit, similar to customer payments
                    IsDirectPayment = true,
                    AmountUsed = 0,
                    PaymentLinkId = 0,
                    PlaceOfSupplyId = 0,
                    TaxId = 0,
                    IsBusinessRegistered = 0,
                    GstTreatment = null,
                    BusinessRegistrationNameId = 0,
                    BusinessRegistrationNo = null,
                    BusinessLegalName = null,
                    BusinessTradeName = null,
                    PanNo = null,
                    TaxAccountId = 0,
                    AmountExcTax = obj.Amount,
                    TaxAmount = 0,
                    IsCancelled = false,
                    PrefixId = 0,
                    IsAdvance = false
                    };

                    oConnectionContext.DbClsCustomerPayment.Add(oCentralPayment);
                    oConnectionContext.SaveChanges();

                    // Update customer's advance balance (unused credits)
                    string balanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + obj.Amount + " where \"UserId\"=" + booking.CustomerId;
                    oConnectionContext.Database.ExecuteSqlCommand(balanceQuery);
                }

                // Update booking payment transaction info if provided
                var oBooking = oConnectionContext.DbClsTableBooking
                    .Where(b => b.BookingId == obj.BookingId)
                    .FirstOrDefault();

                if (oBooking != null)
                {
                    // Calculate deposit paid status dynamically
                    totalPaid = oConnectionContext.DbClsCustomerPayment
                        .Where(cp => cp.BookingId == obj.BookingId &&
                                     cp.IsDeleted == false &&
                                     cp.IsCancelled == false &&
                                     cp.Type == "Booking Deposit Payment")
                        .Select(cp => cp.Amount)
                        .DefaultIfEmpty(0)
                        .Sum();

                    bool depositPaid = totalPaid >= booking.DepositAmount;

                    // PaymentTransactionId and PaymentGatewayType are optional and stored in booking table, not payment table
                    // These properties don't exist in ClsCustomerPaymentVm, so we skip this update
                    // If needed in future, these can be added to ClsCustomerPaymentVm or passed via Notes/ReferenceId
                    // if (depositPaid && string.IsNullOrEmpty(oBooking.PaymentTransactionId) && !string.IsNullOrEmpty(obj.PaymentTransactionId))
                    // {
                    //     oBooking.PaymentTransactionId = obj.PaymentTransactionId;
                    //     oBooking.PaymentGatewayType = obj.PaymentGatewayType;
                    //     oBooking.PaymentDate = CurrentDate;
                    //     oConnectionContext.SaveChanges();
                    // }
                }

                // Activity log
                long paymentIdForLog = 0;
                string logDescription = "";
                if (obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count > 0)
                {
                    paymentIdForLog = obj.CustomerPaymentIds.First().CustomerPaymentId;
                    logDescription = "Credits applied to booking " + booking.BookingNo;
                }
                else if (oCentralPayment != null)
                {
                    paymentIdForLog = oCentralPayment.CustomerPaymentId;
                    logDescription = "Booking Deposit Payment \"" + (obj.ReferenceNo ?? "") + "\" created for booking " + booking.BookingNo;
                }

                if (paymentIdForLog > 0)
                {
                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Booking Payment",
                        CompanyId = obj.CompanyId,
                        Description = logDescription,
                        Id = paymentIdForLog,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);
                }

                data = new
                {
                    Status = 1,
                    Message = obj.CustomerPaymentIds != null && obj.CustomerPaymentIds.Count > 0 
                        ? "Credits applied to booking successfully" 
                        : "Booking payment recorded successfully",
                    Data = new
                    {
                        // Keep the same name used by callers, but now this is CustomerPaymentId
                        BookingPaymentId = oCentralPayment != null ? oCentralPayment.CustomerPaymentId : paymentIdForLog
                    }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> BookingPayments(ClsCustomerPaymentVm obj)
        {
            try
            {
                // Validate input
                if (obj == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Invalid request data",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.BookingId <= 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Booking ID is required",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                if (obj.CompanyId <= 0)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Company ID is required",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Get booking details
                var booking = oConnectionContext.DbClsTableBooking
                    .Where(b => b.BookingId == obj.BookingId && b.CompanyId == obj.CompanyId)
                    .Select(b => new
                    {
                        b.BookingId,
                        b.DepositAmount,
                        b.BookingNo,
                        b.CustomerId
                    })
                    .FirstOrDefault();

                if (booking == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Booking not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Calculate total paid (excluding refunds) from centralized customer payments
                decimal totalPaid = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == obj.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Payment")
                    .Select(cp => cp.Amount)
                    .DefaultIfEmpty(0)
                    .Sum();

                // Calculate total refunded
                decimal totalRefunded = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == obj.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Refund")
                    .Select(cp => cp.Amount)
                    .DefaultIfEmpty(0)
                    .Sum();

                // Net paid amount (payments minus refunds)
                decimal netPaid = totalPaid - totalRefunded;
                if (netPaid < 0) netPaid = 0;

                decimal dueAmount = booking.DepositAmount - netPaid;
                if (dueAmount < 0) dueAmount = 0;

                // Get customer advance balance (unused credits)
                decimal advanceBalance = 0;
                if (booking.CustomerId > 0)
                {
                    advanceBalance = oConnectionContext.DbClsUser
                        .Where(u => u.UserId == booking.CustomerId)
                        .Select(u => u.AdvanceBalance)
                        .FirstOrDefault();
                }

                // Get payment list from centralized customer payments
                var det = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == obj.BookingId && cp.IsDeleted == false && cp.IsCancelled == false &&
                                 (cp.Type == "Booking Deposit Payment" || cp.Type == "Booking Deposit Refund"))
                    .Select(cp => new
                    {
                        Type = cp.Type,
                        RefundOfPaymentId = 0L, // not tracked centrally; kept for compatibility
                        cp.ReferenceNo,
                        AccountName = oConnectionContext.DbClsAccount
                            .Where(a => a.AccountId == cp.AccountId)
                            .Select(a => a.AccountName)
                            .FirstOrDefault(),
                        BookingPaymentId = cp.CustomerPaymentId,
                        CustomerPaymentId = cp.CustomerPaymentId,
                        PaymentDate = cp.PaymentDate,
                        cp.Notes,
                        cp.Amount,
                        cp.AttachDocument,
                        cp.PaymentTypeId,
                        PaymentType = oConnectionContext.DbClsPaymentType
                            .Where(pt => pt.PaymentTypeId == cp.PaymentTypeId)
                            .Select(pt => pt.PaymentType)
                            .FirstOrDefault(),
                        cp.AddedOn,
                        cp.ModifiedOn,
                        AddedByCode = oConnectionContext.DbClsUser
                            .Where(u => u.UserId == cp.AddedBy)
                            .Select(u => u.Username)
                            .FirstOrDefault(),
                        ModifiedByCode = oConnectionContext.DbClsUser
                            .Where(u => u.UserId == cp.ModifiedBy)
                            .Select(u => u.Username)
                            .FirstOrDefault(),
                        cp.ReferenceId,
                        cp.ParentId,
                        cp.IsDirectPayment,
                        IsCancelled = cp.IsCancelled,
                        // Derive payment status and refund flag from Type
                        PaymentStatus = cp.Type == "Booking Deposit Refund" ? "Refunded" : "Paid",
                        PaymentTransactionId = "",
                        PaymentGatewayType = "",
                        IsRefund = (cp.Type == "Booking Deposit Refund"),
                        SalesReturnInvoiceNo = cp.ParentId > 0 ? oConnectionContext.DbClsCustomerPayment
                            .Where(p => p.CustomerPaymentId == cp.ParentId)
                            .Select(p => p.ReferenceNo)
                            .FirstOrDefault() : null
                    })
                    .OrderByDescending(cp => cp.BookingPaymentId)
                    .ToList();

                data = new
                {
                    Status = 1,
                    Message = "found",
                    Data = new
                    {
                        BookingPayments = det,
                        Booking = new
                        {
                            BookingId = booking.BookingId,
                            DepositAmount = booking.DepositAmount,
                            TotalPaid = totalPaid,
                            TotalRefunded = totalRefunded,
                            NetPaid = netPaid,
                            DueAmount = dueAmount,
                            BookingNo = booking.BookingNo,
                            CustomerId = booking.CustomerId,
                            AdvanceBalance = advanceBalance
                        }
                    }
                };
                return await Task.FromResult(Ok(data));
            }
            catch (Exception ex)
            {
                data = new
                {
                    Status = 0,
                    Message = "An error occurred while retrieving booking payments: " + ex.Message,
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> Delete(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Check if payment has refunds in centralized customer payments (booking refunds)
                var hasRefunds = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.BookingId == obj.BookingId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Refund" &&
                                 cp.ParentId == obj.CustomerPaymentId)
                    .Count() > 0;

                if (hasRefunds)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Payment cannot be deleted as one or more refunds have been recorded for this payment",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Fetch centralized payment row
                // Map BookingPaymentId to CustomerPaymentId for backward compatibility
                long paymentId = obj.CustomerPaymentId > 0 ? obj.CustomerPaymentId : (obj as dynamic)?.BookingPaymentId ?? 0;
                var payment = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.CustomerPaymentId == paymentId)
                    .Select(cp => new
                    {
                        cp.BookingId,
                        cp.ReferenceId,
                        cp.ReferenceNo,
                        cp.ParentId,
                        cp.Amount,
                        cp.CustomerId,
                        cp.IsDirectPayment,
                        IsRefund = (cp.Type == "Booking Deposit Refund"),
                        BookingNo = oConnectionContext.DbClsTableBooking
                            .Where(b => b.BookingId == cp.BookingId)
                            .Select(b => b.BookingNo)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                if (payment == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Payment not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Don't allow deleting refunds directly - use RefundDelete instead
                if (payment.IsRefund)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Please use the refund delete option to delete refunds",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Soft-delete centralized customer payment row
                var centralPaymentRow = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.CustomerPaymentId == paymentId)
                    .FirstOrDefault();

                if (centralPaymentRow != null)
                {
                    // If this is a credit payment (has ParentId and IsDirectPayment = false), restore the credit
                    if (payment.ParentId > 0 && !payment.IsDirectPayment)
                    {
                        // Restore parent credit payment's AmountRemaining and AmountUsed
                        string restoreParentQuery = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + payment.Amount + 
                            ",\"AmountUsed\"=\"AmountUsed\"-" + payment.Amount + " where \"CustomerPaymentId\"=" + payment.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(restoreParentQuery);

                        // Restore customer's advance balance
                        string restoreBalanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + payment.Amount + " where \"UserId\"=" + payment.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(restoreBalanceQuery);
                    }

                    centralPaymentRow.IsDeleted = true;
                    centralPaymentRow.ModifiedBy = obj.AddedBy;
                    centralPaymentRow.ModifiedOn = CurrentDate;
                    oConnectionContext.SaveChanges();
                }

                // DepositPaid is now calculated dynamically, no need to update

                // Activity log
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Booking Payment",
                    CompanyId = obj.CompanyId,
                    Description = "Booking Deposit Payment \"" + payment.ReferenceNo + "\" deleted for booking " + payment.BookingNo,
                    Id = paymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Booking payment deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> BookingRefunds(ClsCustomerPaymentVm obj)
        {
            // Get original payment details from centralized customer payments
            // Map RefundOfPaymentId to ParentId for backward compatibility
            long refundOfPaymentId = obj.ParentId > 0 ? obj.ParentId : (obj as dynamic)?.RefundOfPaymentId ?? 0;
            var originalPayment = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.CustomerPaymentId == refundOfPaymentId &&
                             cp.CompanyId == obj.CompanyId &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.Type == "Booking Deposit Payment")
                .Select(cp => new
                {
                    cp.BookingId,
                    cp.Amount,
                    BookingNo = oConnectionContext.DbClsTableBooking
                        .Where(b => b.BookingId == cp.BookingId)
                        .Select(b => b.BookingNo)
                        .FirstOrDefault()
                })
                .FirstOrDefault();

            if (originalPayment == null)
            {
                data = new
                {
                    Status = 0,
                    Message = "Original payment not found",
                    Data = new { }
                };
                return await Task.FromResult(Ok(data));
            }

            // Calculate total refunded amount from centralized customer payments
            decimal totalRefunded = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.BookingId == originalPayment.BookingId &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.Type == "Booking Deposit Refund")
                .Select(cp => cp.Amount)
                .DefaultIfEmpty(0)
                .Sum();

            decimal refundableAmount = originalPayment.Amount - totalRefunded;
            if (refundableAmount < 0) refundableAmount = 0;

            // Get refund list from centralized customer payments
            var det = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.BookingId == originalPayment.BookingId &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.Type == "Booking Deposit Refund")
                .Select(cp => new
                {
                    Type = cp.Type,
                    cp.ReferenceNo,
                    AccountName = oConnectionContext.DbClsAccount
                        .Where(a => a.AccountId == cp.AccountId)
                        .Select(a => a.AccountName)
                        .FirstOrDefault(),
                    BookingPaymentId = cp.CustomerPaymentId,
                    PaymentDate = cp.PaymentDate,
                    cp.Notes,
                    cp.Amount,
                    cp.AttachDocument,
                    cp.PaymentTypeId,
                    PaymentType = oConnectionContext.DbClsPaymentType
                        .Where(pt => pt.PaymentTypeId == cp.PaymentTypeId)
                        .Select(pt => pt.PaymentType)
                        .FirstOrDefault(),
                    cp.AddedOn,
                    cp.ModifiedOn,
                    AddedByCode = oConnectionContext.DbClsUser
                        .Where(u => u.UserId == cp.AddedBy)
                        .Select(u => u.Username)
                        .FirstOrDefault(),
                    ModifiedByCode = oConnectionContext.DbClsUser
                        .Where(u => u.UserId == cp.ModifiedBy)
                        .Select(u => u.Username)
                        .FirstOrDefault(),
                    cp.ReferenceId,
                    PaymentStatus = "Refunded",
                    IsRefund = true
                })
                .OrderByDescending(cp => cp.BookingPaymentId)
                .ToList();

            data = new
            {
                Status = 1,
                Message = "found",
                Data = new
                {
                    BookingRefunds = det,
                    OriginalPayment = new
                    {
                        Amount = originalPayment.Amount,
                        TotalRefunded = totalRefunded,
                        RefundableAmount = refundableAmount,
                        BookingNo = originalPayment.BookingNo
                    }
                }
            };
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> InsertBookingRefund(ClsCustomerPaymentVm obj)
        {
            bool isError = false;
            List<ClsError> errors = new List<ClsError>();

            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Get original payment details from centralized customer payments
                // Map RefundOfPaymentId to ParentId for backward compatibility
                long refundOfPaymentId = obj.ParentId > 0 ? obj.ParentId : (obj as dynamic)?.RefundOfPaymentId ?? 0;
                var originalPayment = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.CustomerPaymentId == refundOfPaymentId &&
                                 cp.CompanyId == obj.CompanyId &&
                                 cp.IsDeleted == false &&
                                 cp.IsCancelled == false &&
                                 cp.Type == "Booking Deposit Payment")
                    .Select(cp => new
                    {
                        cp.BookingId,
                        cp.CustomerId,
                        cp.BranchId,
                        cp.Amount,
                        cp.ParentId,
                        cp.IsDirectPayment,
                        cp.AmountRemaining,
                        BookingNo = oConnectionContext.DbClsTableBooking
                            .Where(b => b.BookingId == cp.BookingId)
                            .Select(b => b.BookingNo)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                if (originalPayment == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Original payment not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

            // Calculate total refunded amount from centralized customer payments
            decimal totalRefundedForPayment = oConnectionContext.DbClsCustomerPayment
                .Where(cp => cp.BookingId == originalPayment.BookingId &&
                             cp.IsDeleted == false &&
                             cp.IsCancelled == false &&
                             cp.Type == "Booking Deposit Refund")
                .Select(cp => cp.Amount)
                .DefaultIfEmpty(0)
                .Sum();

                decimal refundableAmount = originalPayment.Amount - totalRefundedForPayment;

                // Validation common to both cash and credit-based deposits
                if (obj.Amount == 0)
                {
                    errors.Add(new ClsError { Message = "This field is required", Id = "divAmount" });
                    isError = true;
                }

                // Branch: check if deposit is stored as unused credit (AmountRemaining > 0) 
                // or if it was originally applied from credits (ParentId > 0 && !IsDirectPayment)
                bool isUnusedCreditDeposit = originalPayment.AmountRemaining > 0 && originalPayment.ParentId == 0;
                bool isCreditBasedDeposit = originalPayment.ParentId > 0 && originalPayment.IsDirectPayment == false;
                bool shouldRestoreCredits = isUnusedCreditDeposit || isCreditBasedDeposit;

                if (!shouldRestoreCredits)
                {
                    // Existing behaviour for direct payments: cash refund via Booking Deposit Refund
                    if (obj.Amount > refundableAmount)
                    {
                        errors.Add(new ClsError { Message = "Refund amount cannot exceed refundable amount (" + refundableAmount.ToString("N2") + ")", Id = "divAmount" });
                        isError = true;
                    }

                    if (obj.PaymentDate == DateTime.MinValue)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divRefundDate" });
                        isError = true;
                    }

                    if (obj.PaymentTypeId == 0)
                    {
                        errors.Add(new ClsError { Message = "This field is required", Id = "divPaymentType" });
                        isError = true;
                    }

                    if (isError)
                    {
                        data = new
                        {
                            Status = 2,
                            Message = "Validation failed",
                            Errors = errors,
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    // Generate reference number if not provided
                    if (string.IsNullOrEmpty(obj.ReferenceNo))
                    {
                        long PrefixId = oConnectionContext.DbClsBranch
                            .Where(a => a.BranchId == originalPayment.BranchId)
                            .Select(a => a.PrefixId)
                            .FirstOrDefault();

                        var prefixSettings = (from a in oConnectionContext.DbClsPrefixMaster
                                              join b in oConnectionContext.DbClsPrefixUserMap
                                               on a.PrefixMasterId equals b.PrefixMasterId
                                              where a.IsActive == true && a.IsDeleted == false &&
                                              b.CompanyId == obj.CompanyId && b.IsActive == true
                                              && b.IsDeleted == false && a.PrefixType.ToLower() == "payment"
                                              && b.PrefixId == PrefixId
                                              select new
                                              {
                                                  b.PrefixUserMapId,
                                                  b.Prefix,
                                                  b.NoOfDigits,
                                                  b.Counter
                                              }).FirstOrDefault();

                        if (prefixSettings != null)
                        {
                            obj.ReferenceNo = prefixSettings.Prefix + prefixSettings.Counter.ToString().PadLeft(prefixSettings.NoOfDigits, '0');
                        }
                        else
                        {
                            obj.ReferenceNo = "REF-" + originalPayment.BookingNo + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                        }
                    }

                    // Insert centralized refund record into tblCustomerPayment
                    string refundReferenceId = oCommonController.CreateToken();

                    ClsCustomerPayment oCentralRefund = new ClsCustomerPayment()
                    {
                        CustomerId = originalPayment.CustomerId,
                        BookingId = originalPayment.BookingId,
                        SalesId = 0,
                        SalesReturnId = 0,
                        PaymentDate = obj.PaymentDate.AddHours(5).AddMinutes(30),
                        PaymentTypeId = obj.PaymentTypeId,
                        Amount = obj.Amount,
                        Notes = string.IsNullOrEmpty(obj.Notes)
                            ? "Booking Deposit Refund for " + originalPayment.BookingNo
                            : obj.Notes,
                        CompanyId = obj.CompanyId,
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = obj.AddedBy,
                        AddedOn = CurrentDate,
                        ModifiedBy = obj.AddedBy,
                        ModifiedOn = CurrentDate,
                        AttachDocument = obj.AttachDocument,
                        Type = "Booking Deposit Refund",
                        AccountId = obj.AccountId,
                        BranchId = originalPayment.BranchId,
                        ReferenceNo = obj.ReferenceNo,
                        IsDebit = 1, // refund going out
                        ParentId = refundOfPaymentId,
                        ReferenceId = refundReferenceId,
                        JournalAccountId = obj.JournalAccountId,
                        AmountRemaining = 0,
                        IsDirectPayment = true,
                        AmountUsed = 0,
                        PaymentLinkId = 0,
                        PlaceOfSupplyId = 0,
                        TaxId = 0,
                        IsBusinessRegistered = 0,
                        GstTreatment = null,
                        BusinessRegistrationNameId = 0,
                        BusinessRegistrationNo = null,
                        BusinessLegalName = null,
                        BusinessTradeName = null,
                        PanNo = null,
                        TaxAccountId = 0,
                        AmountExcTax = obj.Amount,
                        TaxAmount = 0,
                        IsCancelled = false,
                        PrefixId = 0,
                        IsAdvance = false
                    };

                    oConnectionContext.DbClsCustomerPayment.Add(oCentralRefund);
                    oConnectionContext.SaveChanges();

                    // Activity log
                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Booking Refund",
                        CompanyId = obj.CompanyId,
                        Description = "Booking Deposit Refund \"" + obj.ReferenceNo + "\" created for booking " + originalPayment.BookingNo,
                        Id = oCentralRefund.CustomerPaymentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Insert"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                    data = new
                    {
                        Status = 1,
                        Message = "Booking refund recorded successfully",
                        Data = new
                        {
                            BookingPaymentId = oCentralRefund.CustomerPaymentId
                        }
                    };
                }
                else
                {
                    // New behaviour: restore credits instead of issuing cash refund.
                    // For deposits stored as unused credits or credit-based deposits, restore the credits.
                    // For simplicity and consistency with cancellation, only full restoration is supported.
                    if (totalRefundedForPayment > 0)
                    {
                        errors.Add(new ClsError { Message = "Cannot restore credits because cash refunds have already been recorded for this deposit", Id = "divAmount" });
                        isError = true;
                    }

                    if (obj.Amount != originalPayment.Amount)
                    {
                        errors.Add(new ClsError { Message = "For credit-based deposits, refund amount must be equal to the full deposit amount", Id = "divAmount" });
                        isError = true;
                    }

                    if (isError)
                    {
                        data = new
                        {
                            Status = 2,
                            Message = "Validation failed",
                            Errors = errors,
                            Data = new { }
                        };
                        return await Task.FromResult(Ok(data));
                    }

                    if (isCreditBasedDeposit)
                    {
                        // Restore parent credit payment's AmountRemaining and AmountUsed
                        string restoreParentQuery = "update \"tblCustomerPayment\" set \"AmountRemaining\"=\"AmountRemaining\"+" + obj.Amount +
                            ",\"AmountUsed\"=\"AmountUsed\"-" + obj.Amount + " where \"CustomerPaymentId\"=" + originalPayment.ParentId;
                        oConnectionContext.Database.ExecuteSqlCommand(restoreParentQuery);

                        // Restore customer's advance balance
                        string restoreBalanceQuery = "update \"tblUser\" set \"AdvanceBalance\"=\"AdvanceBalance\"+" + obj.Amount + " where \"UserId\"=" + originalPayment.CustomerId;
                        oConnectionContext.Database.ExecuteSqlCommand(restoreBalanceQuery);
                    }
                    // For unused credit deposits (isUnusedCreditDeposit), the credit is already in the customer's advance balance
                    // since we added it when the deposit was created. The deposit payment's AmountRemaining also reflects this.
                    // When refunding, we just soft-delete the deposit payment record. The customer's advance balance remains
                    // unchanged, as the credit is already available for use elsewhere (the credit was never tied to this booking).

                    // Soft-delete the booking deposit payment row
                    var depositPaymentRow = oConnectionContext.DbClsCustomerPayment
                        .Where(cp => cp.CustomerPaymentId == refundOfPaymentId)
                        .FirstOrDefault();
                    if (depositPaymentRow != null)
                    {
                        depositPaymentRow.IsDeleted = true;
                        depositPaymentRow.ModifiedBy = obj.AddedBy;
                        depositPaymentRow.ModifiedOn = CurrentDate;
                        oConnectionContext.SaveChanges();
                    }

                    // Activity log
                    ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                    {
                        AddedBy = obj.AddedBy,
                        Browser = obj.Browser,
                        Category = "Booking Refund",
                        CompanyId = obj.CompanyId,
                        Description = "Booking deposit credits restored for booking " + originalPayment.BookingNo,
                        Id = refundOfPaymentId,
                        IpAddress = obj.IpAddress,
                        Platform = obj.Platform,
                        Type = "Update"
                    };
                    oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                    data = new
                    {
                        Status = 1,
                        Message = "Booking deposit credits restored successfully",
                        Data = new
                        {
                        }
                    };
                }

                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }

        [HttpPost]
        public async Task<IHttpActionResult> RefundDelete(ClsCustomerPaymentVm obj)
        {
            var CurrentDate = oCommonController.CurrentDate(obj.CompanyId);
            using (TransactionScope dbContextTransaction = new TransactionScope())
            {
                // Map BookingPaymentId to CustomerPaymentId for backward compatibility
                long refundPaymentId = obj.CustomerPaymentId > 0 ? obj.CustomerPaymentId : (obj as dynamic)?.BookingPaymentId ?? 0;
                var refund = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.CustomerPaymentId == refundPaymentId)
                    .Select(cp => new
                    {
                        cp.BookingId,
                        cp.ReferenceNo,
                        BookingNo = oConnectionContext.DbClsTableBooking
                            .Where(b => b.BookingId == cp.BookingId)
                            .Select(b => b.BookingNo)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                if (refund == null)
                {
                    data = new
                    {
                        Status = 0,
                        Message = "Refund not found",
                        Data = new { }
                    };
                    return await Task.FromResult(Ok(data));
                }

                // Soft-delete centralized refund row
                var centralRefund = oConnectionContext.DbClsCustomerPayment
                    .Where(cp => cp.CustomerPaymentId == refundPaymentId)
                    .FirstOrDefault();

                if (centralRefund != null)
                {
                    centralRefund.IsDeleted = true;
                    centralRefund.ModifiedBy = obj.AddedBy;
                    centralRefund.ModifiedOn = CurrentDate;
                    oConnectionContext.SaveChanges();
                }

                // DepositPaid is now calculated dynamically, no need to update

                // Activity log
                ClsActivityLogVm oClsActivityLogVm = new ClsActivityLogVm()
                {
                    AddedBy = obj.AddedBy,
                    Browser = obj.Browser,
                    Category = "Booking Refund",
                    CompanyId = obj.CompanyId,
                    Description = "Booking Deposit Refund \"" + refund.ReferenceNo + "\" deleted for booking " + refund.BookingNo,
                    Id = refundPaymentId,
                    IpAddress = obj.IpAddress,
                    Platform = obj.Platform,
                    Type = "Delete"
                };
                oCommonController.InsertActivityLog(oClsActivityLogVm, CurrentDate);

                data = new
                {
                    Status = 1,
                    Message = "Booking refund deleted successfully",
                    Data = new { }
                };
                dbContextTransaction.Complete();
            }
            return await Task.FromResult(Ok(data));
        }
    }
}

