using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EquiBillBook.Models
{
    [Table("public.tblReminderModulesSettings")]
    public class ClsReminderModulesSettings
    {
        [Key]
        public long ReminderModulesSettingsId { get; set; }
        public long ReminderModulesId { get; set; }
        public long ReminderModulesDetailsId { get; set; }
        public string ReminderType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public string ReminderTo { get; set; }
        public int ReminderInDays { get; set; }
        public string ReminderBeforeAfter { get; set; }
        public decimal TotalDue { get; set; }
    }

    public class ClsReminderModulesSettingsVm
    {
        public long ReminderModulesSettingsId { get; set; }
        public long ReminderModulesId { get; set; }
        public long ReminderModulesDetailsId { get; set; }
        public string ReminderType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public string ReminderTo { get; set; }
        public int ReminderInDays { get; set; }
        public string ReminderBeforeAfter { get; set; }
        public decimal TotalDue { get; set; }
        public string UserType { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public string[] ReminderExceptionContactsArray { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public long Id { get; set; }
    }

}