using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblAccountSettings")]
    public class ClsAccountSettings
    {
        [Key]
        public long AccountSettingsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public DateTime MigrationDate { get; set; }
    }

    public class ClsAccountSettingsVm
    {
        public long AccountSettingsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime? AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public DateTime MigrationDate { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}