using EquiBillBook.Models;
using System;
using System.Linq;
using System.Web.Configuration;

namespace EquiBillBook.Controllers.WebApi
{
    /// <summary>
    /// Helper class for sending reward points notifications
    /// </summary>
    public class RewardPointsNotificationHelper
    {
        private ConnectionContext oConnectionContext = new ConnectionContext();
        private CommonController oCommonController = new CommonController();
        private EmailController oEmailController = new EmailController();
        public string webUrl = WebConfigurationManager.AppSettings["webUrl"];

        /// <summary>
        /// Send notification when points are earned
        /// </summary>
        public void SendPointsEarnedNotification(long customerId, long companyId, long salesId, decimal points, string invoiceNo, DateTime currentDate, string domain)
        {
            try
            {
                // Get customer details
                var customer = oConnectionContext.DbClsUser
                    .Where(a => a.UserId == customerId && a.CompanyId == companyId && !a.IsDeleted)
                    .Select(a => new
                    {
                        a.Name,
                        a.EmailId,
                        a.MobileNo
                    })
                    .FirstOrDefault();

                if (customer == null) return;

                // Get reward point settings
                var settings = oConnectionContext.DbClsRewardPointSettings
                    .Where(a => a.CompanyId == companyId && a.EnableRewardPoint && !a.IsDeleted)
                    .Select(a => new
                    {
                        a.DisplayName
                    })
                    .FirstOrDefault();

                if (settings == null) return;

                string displayName = string.IsNullOrEmpty(settings.DisplayName) ? "Points" : settings.DisplayName;

                // Get business details
                var business = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.CompanyId == companyId)
                    .Select(a => new
                    {
                        a.BusinessName,
                        a.BusinessLogo
                    })
                    .FirstOrDefault();

                // Prepare notification content
                string subject = $"You've Earned {points} {displayName}!";
                string emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Congratulations {customer.Name}!</h2>
                        <p>You've earned <strong>{points} {displayName}</strong> on your purchase.</p>
                        <p><strong>Invoice No:</strong> {invoiceNo}</p>
                        <p>Your total available {displayName.ToLower()} balance has been updated.</p>
                        <p>Thank you for your purchase!</p>
                        <br/>
                        <p>Best regards,<br/>{business?.BusinessName ?? "Team"}</p>
                    </body>
                    </html>";

                string smsBody = $"Hi {customer.Name}, You've earned {points} {displayName} on invoice {invoiceNo}. Thank you!";

                // Send email if customer has email
                if (!string.IsNullOrEmpty(customer.EmailId))
                {
                    long emailSettingsId = oConnectionContext.DbClsEmailSettings
                        .Where(a => a.CompanyId == companyId && a.IsDefault && !a.IsDeleted && a.IsActive)
                        .Select(a => a.EmailSettingsId)
                        .FirstOrDefault();

                    oEmailController.EmailFunction(companyId, customer.EmailId, subject, emailBody, "", "", 0, currentDate, salesId, "Reward Points Earned", domain, emailSettingsId);
                }

                // Note: SMS and WhatsApp can be added here using similar pattern
                // For now, we're just sending email notifications
            }
            catch (Exception ex)
            {
                // Log error but don't throw - notifications shouldn't break the main flow
                System.Diagnostics.Debug.WriteLine($"Error sending points earned notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification when points are redeemed
        /// </summary>
        public void SendPointsRedeemedNotification(long customerId, long companyId, long salesId, decimal points, decimal discountAmount, string invoiceNo, DateTime currentDate, string domain)
        {
            try
            {
                var customer = oConnectionContext.DbClsUser
                    .Where(a => a.UserId == customerId && a.CompanyId == companyId && !a.IsDeleted)
                    .Select(a => new
                    {
                        a.Name,
                        a.EmailId,
                        a.MobileNo
                    })
                    .FirstOrDefault();

                if (customer == null) return;

                var settings = oConnectionContext.DbClsRewardPointSettings
                    .Where(a => a.CompanyId == companyId && a.EnableRewardPoint && !a.IsDeleted)
                    .Select(a => new
                    {
                        a.DisplayName
                    })
                    .FirstOrDefault();

                if (settings == null) return;

                string displayName = string.IsNullOrEmpty(settings.DisplayName) ? "Points" : settings.DisplayName;

                var business = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.CompanyId == companyId)
                    .Select(a => new
                    {
                        a.BusinessName
                    })
                    .FirstOrDefault();

                string subject = $"You've Redeemed {points} {displayName}";
                string emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Thank you {customer.Name}!</h2>
                        <p>You've successfully redeemed <strong>{points} {displayName}</strong> for a discount of <strong>₹{discountAmount:N2}</strong>.</p>
                        <p><strong>Invoice No:</strong> {invoiceNo}</p>
                        <p>Your {displayName.ToLower()} balance has been updated.</p>
                        <br/>
                        <p>Best regards,<br/>{business?.BusinessName ?? "Team"}</p>
                    </body>
                    </html>";

                string smsBody = $"Hi {customer.Name}, You've redeemed {points} {displayName} for ₹{discountAmount:N2} discount on invoice {invoiceNo}. Thank you!";

                if (!string.IsNullOrEmpty(customer.EmailId))
                {
                    long emailSettingsId = oConnectionContext.DbClsEmailSettings
                        .Where(a => a.CompanyId == companyId && a.IsDefault && !a.IsDeleted && a.IsActive)
                        .Select(a => a.EmailSettingsId)
                        .FirstOrDefault();

                    oEmailController.EmailFunction(companyId, customer.EmailId, subject, emailBody, "", "", 0, currentDate, salesId, "Reward Points Redeemed", domain, emailSettingsId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending points redeemed notification: {ex.Message}");
            }
        }

        /// <summary>
        /// Send notification when points are about to expire
        /// </summary>
        public void SendPointsExpiringNotification(long customerId, long companyId, decimal points, DateTime expiryDate, DateTime currentDate, string domain)
        {
            try
            {
                var customer = oConnectionContext.DbClsUser
                    .Where(a => a.UserId == customerId && a.CompanyId == companyId && !a.IsDeleted)
                    .Select(a => new
                    {
                        a.Name,
                        a.EmailId,
                        a.MobileNo
                    })
                    .FirstOrDefault();

                if (customer == null) return;

                var settings = oConnectionContext.DbClsRewardPointSettings
                    .Where(a => a.CompanyId == companyId && a.EnableRewardPoint && !a.IsDeleted)
                    .Select(a => new
                    {
                        a.DisplayName
                    })
                    .FirstOrDefault();

                if (settings == null) return;

                string displayName = string.IsNullOrEmpty(settings.DisplayName) ? "Points" : settings.DisplayName;

                var business = oConnectionContext.DbClsBusinessSettings
                    .Where(a => a.CompanyId == companyId)
                    .Select(a => new
                    {
                        a.BusinessName
                    })
                    .FirstOrDefault();

                int daysUntilExpiry = (expiryDate - currentDate).Days;

                string subject = $"Your {displayName} Are Expiring Soon!";
                string emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Reminder: {displayName} Expiring Soon</h2>
                        <p>Hi {customer.Name},</p>
                        <p>This is a reminder that <strong>{points} {displayName}</strong> will expire on <strong>{expiryDate:dd-MM-yyyy}</strong>.</p>
                        <p>You have <strong>{daysUntilExpiry} days</strong> left to use them!</p>
                        <p>Don't miss out - use your {displayName.ToLower()} on your next purchase.</p>
                        <br/>
                        <p>Best regards,<br/>{business?.BusinessName ?? "Team"}</p>
                    </body>
                    </html>";

                string smsBody = $"Hi {customer.Name}, {points} {displayName} expiring on {expiryDate:dd-MM-yyyy}. Use them now!";

                if (!string.IsNullOrEmpty(customer.EmailId))
                {
                    long emailSettingsId = oConnectionContext.DbClsEmailSettings
                        .Where(a => a.CompanyId == companyId && a.IsDefault && !a.IsDeleted && a.IsActive)
                        .Select(a => a.EmailSettingsId)
                        .FirstOrDefault();

                    oEmailController.EmailFunction(companyId, customer.EmailId, subject, emailBody, "", "", 0, currentDate, 0, "Reward Points Expiring", domain, emailSettingsId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending points expiring notification: {ex.Message}");
            }
        }
    }
}

