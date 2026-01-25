using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblNotificationModulesSettings")]
    public class ClsNotificationModulesSettings
    {
        [Key]
        public long NotificationModulesSettingsId { get; set; }
        public long NotificationModulesId { get; set; }
        public long NotificationModulesDetailsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool AutoSendEmail { get; set; }
        public string EmailSubject { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string EmailBody { get; set; }
        public bool AutoSendSms { get; set; }
        public string SmsBody { get; set; }
        public bool AutoSendWhatsapp { get; set; }
        public string WhatsappBody { get; set; }
        public long NotificationTemplatesId { get; set; }

    }

    public class ClsNotificationModulesSettingsVm
    {
        public long NotificationModulesSettingsId { get; set; }
        public long NotificationModulesId { get; set; }
        public long NotificationModulesDetailsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool AutoSendEmail { get; set; }
        public string EmailSubject { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        [AllowHtml]
        public string EmailBody { get; set; }
        public bool AutoSendSms { get; set; }
        [AllowHtml]
        public string SmsBody { get; set; }
        public bool AutoSendWhatsapp { get; set; }
        [AllowHtml]
        public string WhatsappBody { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public string Name { get; set; }
        public long Id { get; set; }
        public long NotificationTemplatesId { get; set; }
    }

}