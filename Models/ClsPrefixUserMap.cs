using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPrefixUserMap")]
    public class ClsPrefixUserMap
    {
        [Key]
        public long PrefixUserMapId { get; set; }
        public long PrefixMasterId { get; set; }
        public long PrefixId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Prefix { get; set; }
        public int NoOfDigits { get; set; }
        public long Counter { get; set; }
    }

    public class ClsPrefixUserMapVm
    {
        public long PrefixUserMapId { get; set; }
        public long PrefixMasterId { get; set; }
        public long PrefixId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Prefix { get; set; }
        public int NoOfDigits { get; set; }
        public long Counter { get; set; }
        public long Id { get; set; }
    }

}