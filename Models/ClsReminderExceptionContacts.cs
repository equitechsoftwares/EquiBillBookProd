using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblReminderExceptionContacts")]
    public class ClsReminderExceptionContacts
    {
        [Key]
        public long ReminderExceptionContactsId { get; set; }
        public long ReminderModulesSettingsId { get; set; }
        public long UserId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsReminderExceptionContactsVm
    {
        public long ReminderExceptionContactsId { get; set; }
        public long ReminderModulesSettingsId { get; set; }
        public long UserId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

}