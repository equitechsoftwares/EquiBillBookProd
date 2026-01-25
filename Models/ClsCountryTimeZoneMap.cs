using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCountryTimeZoneMap")]
    public class ClsCountryTimeZoneMap
    {
        [Key]
        public long CountryTimeZoneMapId { get; set; }
        public long CountryId { get; set; }
        public long TimeZoneId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsCountryTimeZoneMapVm
    {
        public long CountryTimeZoneMapId { get; set; }
        public long CountryId { get; set; }
        public long TimeZoneId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Domain { get; set; }
    }

}