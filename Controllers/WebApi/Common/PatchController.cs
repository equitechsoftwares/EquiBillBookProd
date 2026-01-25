using EquiBillBook.Filters;
using EquiBillBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EquiBillBook.Controllers.WebApi.Common
{
    [ExceptionHandler]
    public class PatchController : ApiController
    {
        ConnectionContext oConnectionContext = new ConnectionContext();
        CommonController oCommonController = new CommonController();

        public void NotificationTemplateSetup()
        {           
            var NotificationModulesDetails = oConnectionContext.DbClsNotificationTemplates.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a =>
               new
               {
                   a.Sequence,
                   a.NotificationModulesDetailsId,
                   a.NotificationModulesId,
                   a.NotificationTemplatesId,
                   a.EmailSubject,
                   a.EmailBody,
                   a.SmsBody,
                   a.WhatsappBody
               }).OrderBy(a => a.Sequence).ToList();

            var users = oConnectionContext.DbClsUser.Where(a => a.UserType == "User" && a.IsCompany == true).ToList();
            foreach (var user in users)
            {
                var CurrentDate = oCommonController.CurrentDate(user.CompanyId);
                foreach (var item in NotificationModulesDetails)
                {
                    ClsNotificationModulesSettings oClsNotificationModulesSettings = new ClsNotificationModulesSettings()
                    {
                        IsActive = true,
                        IsDeleted = false,
                        AddedBy = user.UserId,
                        AddedOn = CurrentDate,
                        CompanyId = user.UserId,
                        EmailBody = item.EmailBody,
                        EmailSubject = item.EmailSubject,
                        SmsBody = item.SmsBody,
                        WhatsappBody = item.WhatsappBody,
                        NotificationModulesId = item.NotificationModulesId,
                        NotificationModulesDetailsId = item.NotificationModulesDetailsId,
                        NotificationTemplatesId = item.NotificationTemplatesId,
                    };
                    oConnectionContext.DbClsNotificationModulesSettings.Add(oClsNotificationModulesSettings);
                    oConnectionContext.SaveChanges();
                }
            }
            
        }

        public void ReminderTemplateSetup()
        {
            var ReminderModulesDetails = oConnectionContext.DbClsReminderModulesDetails.Where(a => a.IsActive == true && a.IsDeleted == false).Select(a =>
               new
               {
                   a.Sequence,
                   a.ReminderModulesDetailsId,
                   a.ReminderModulesId,
                   a.EmailSubject,
                   a.EmailBody,
                   a.SmsBody,
                   a.WhatsappBody,
                   a.ReminderType,
                   a.Name,
                   a.Description,
                   a.ReminderBeforeAfter
               }).OrderBy(a => a.Sequence).ToList();

            var users = oConnectionContext.DbClsUser.Where(a => a.UserType == "User" && a.IsCompany == true).ToList();
            foreach (var user in users)
            {
                var CurrentDate = oCommonController.CurrentDate(user.CompanyId);

                foreach (var item in ReminderModulesDetails)
                {
                    ClsReminderModulesSettings oClsReminderModulesSettings = new ClsReminderModulesSettings()
                    {
                        AddedBy = user.UserId,
                        AddedOn = CurrentDate,
                        IsActive = true,
                        IsDeleted = false,
                        CompanyId = user.UserId,
                        ReminderModulesId = item.ReminderModulesId,
                        ReminderModulesDetailsId = item.ReminderModulesDetailsId,
                        ReminderType = item.ReminderType,
                        Name = item.Name,
                        Description = item.Description,
                        ReminderBeforeAfter = item.ReminderBeforeAfter,
                        ReminderInDays = 0,
                        ReminderTo = "Customer",
                        TotalDue = 0,
                        AutoSendEmail = false,
                        AutoSendSms = false,
                        AutoSendWhatsapp = false,
                        BCC = "",
                        CC = "",
                        EmailBody = item.EmailBody,
                        EmailSubject = item.EmailSubject,
                        SmsBody = item.SmsBody,
                        WhatsappBody = item.WhatsappBody
                    };
                    oConnectionContext.DbClsReminderModulesSettings.Add(oClsReminderModulesSettings);
                    oConnectionContext.SaveChanges();
                }
            }
        }

        public void RestaurantSettingsSetup()
        {
            // Get all companies
            var users = oConnectionContext.DbClsUser.Where(a => a.UserType == "User" && a.IsCompany == true).ToList();
            
            foreach (var user in users)
            {
                var CurrentDate = oCommonController.CurrentDate(user.CompanyId);
                
                // Get all branches for this company
                var branches = oConnectionContext.DbClsBranch.Where(a => a.CompanyId == user.CompanyId && a.IsDeleted == false).ToList();
                
                foreach (var branch in branches)
                {
                    // Check if restaurant settings already exist for this branch
                    var existingSettings = oConnectionContext.DbClsRestaurantSettings
                        .Where(a => a.CompanyId == user.CompanyId && a.BranchId == branch.BranchId && a.IsDeleted == false)
                        .FirstOrDefault();
                    
                    // Only create if settings don't exist
                    if (existingSettings == null)
                    {
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
                            BranchId = branch.BranchId,
                            CompanyId = user.CompanyId,
                            IsActive = true,
                            IsDeleted = false,
                            AddedBy = user.UserId,
                            AddedOn = CurrentDate,
                            ModifiedBy = user.UserId
                        };
                        oConnectionContext.DbClsRestaurantSettings.Add(defaultRestaurantSettings);
                        oConnectionContext.SaveChanges();
                    }
                }
            }
        }

        public void PrefixUserMapSetup()
        {
            // Get all prefixes from PrefixMaster where PrefixType is "Table Booking" or "KOT"
            var newPrefixMasters = oConnectionContext.DbClsPrefixMaster
                .Where(a => a.IsActive == true && a.IsDeleted == false 
                    //&& (a.PrefixType == "Table Booking" || a.PrefixType == "KOT")
                    )
                .Select(a => new
                {
                    a.PrefixMasterId,
                    a.Prefix,
                    a.NoOfDigits
                }).ToList();

            // Get all companies
            var users = oConnectionContext.DbClsUser.Where(a => a.UserType == "User" && a.IsCompany == true).ToList();

            foreach (var user in users)
            {
                var CurrentDate = oCommonController.CurrentDate(user.CompanyId);

                // Get all PrefixIds for this company
                var prefixIds = oConnectionContext.DbClsPrefix
                    .Where(a => a.CompanyId == user.CompanyId && a.IsDeleted == false)
                    .Select(a => a.PrefixId)
                    .ToList();

                // Process each prefix for this company
                foreach (var PrefixId in prefixIds)
                {
                    foreach (var prefixMaster in newPrefixMasters)
                    {
                        // Check if PrefixUserMap already exists for this combination
                        var existingPrefixUserMap = oConnectionContext.DbClsPrefixUserMap
                            .Where(a => a.CompanyId == user.CompanyId 
                                && a.PrefixId == PrefixId
                                && a.PrefixMasterId == prefixMaster.PrefixMasterId
                                && a.IsDeleted == false)
                            .FirstOrDefault();

                        // Only create if it doesn't exist
                        if (existingPrefixUserMap == null)
                        {
                            ClsPrefixUserMap oClsPrefixUserMap = new ClsPrefixUserMap()
                            {
                                AddedBy = user.UserId,
                                AddedOn = CurrentDate,
                                IsActive = true,
                                IsDeleted = false,
                                CompanyId = user.CompanyId,
                                Counter = 1,
                                PrefixMasterId = prefixMaster.PrefixMasterId,
                                NoOfDigits = prefixMaster.NoOfDigits,
                                Prefix = prefixMaster.Prefix,
                                PrefixId = PrefixId
                            };
                            oConnectionContext.DbClsPrefixUserMap.Add(oClsPrefixUserMap);
                            oConnectionContext.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
