using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblNotificationModulesDetails")]
    public class ClsNotificationModulesDetails
    {
        [Key]
        public long NotificationModulesDetailsId { get; set; }
        public long NotificationModulesId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public string AvailableTags { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsNotificationModulesDetailsVm
    {
        public long NotificationModulesDetailsId { get; set; }
        public long NotificationModulesId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Sequence { get; set; }
        public string AvailableTags { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public List<ClsNotificationTemplatesVm> NotificationTemplates { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string SmsBody { get; set; }
        public string WhatsappBody { get; set; }
    }

}