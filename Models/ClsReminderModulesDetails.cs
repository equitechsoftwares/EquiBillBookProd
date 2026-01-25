using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblReminderModulesDetails")]
    public class ClsReminderModulesDetails
    {
        [Key]
        public long ReminderModulesDetailsId { get; set; }
        public long ReminderModulesId { get; set; }
        public string ReminderType { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string SmsBody { get; set; }
        public string WhatsappBody { get; set; }
        public string ReminderBeforeAfter { get; set; }
    }

    public class ClsReminderModulesDetailsVm
    {
        public long ReminderModulesDetailsId { get; set; }
        public long ReminderModulesId { get; set; }
        public string ReminderType { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string SmsBody { get; set; }
        public string WhatsappBody { get; set; }
        public string ReminderBeforeAfter { get; set; }
    }

}