using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTimeZone")]
    public class ClsTimeZone
    {
        [Key]
        public long TimeZoneId { get; set; }
        public string DisplayName { get; set; }
        public string StandardName { get; set; }
        public string DaylightName { get; set; }
        public bool SupportsDaylightSavingTime { get; set; }
        public string o1String { get; set; }
        public string o2String { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsTimeZoneVm
    {
        public long TimeZoneId { get; set; }
        public string DisplayName { get; set; }
        public string StandardName { get; set; }
        public string DaylightName { get; set; }
        public bool SupportsDaylightSavingTime { get; set; }
        public string o1String { get; set; }
        public string o2String { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Utc { get; set; }
        public long CountryTimeZoneMapId { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }

}