using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblReminderModules")]
    public class ClsReminderModules
    {
        [Key]
        public long ReminderModulesId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AvailableTags { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string SmsBody { get; set; }
        public string WhatsappBody { get; set; }
    }

    public class ClsReminderModulesVm
    {
        public long ReminderModulesId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AvailableTags { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string SmsBody { get; set; }
        public string WhatsappBody { get; set; }
        public List<ClsReminderModulesSettingsVm> ReminderModulesSettings { get; set; }
        public long CompanyId { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}