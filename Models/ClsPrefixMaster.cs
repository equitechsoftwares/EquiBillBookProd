using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPrefixMaster")]
    public class ClsPrefixMaster
    {
        [Key]
        public long PrefixMasterId { get; set; }
        public string PrefixType { get; set; }
        public string PrefixTitle { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string Prefix { get; set; }
        public int NoOfDigits { get; set; }
        public string MenuType { get; set; }
        public int Sequence { get; set; }
    }

    public class ClsPrefixMasterVm
    {
        public long PrefixMasterId { get; set; }
        public string PrefixType { get; set; }
        public string PrefixTitle { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string Prefix { get; set; }
        public int NoOfDigits { get; set; }
        public string MenuType { get; set; }
        public long CompanyId { get; set; }
        public long AddedBy { get; set; }
        public long PrefixUserMapId { get; set; }
        public IList<ClsPrefixUserMapVm> PrefixUserMaps { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public int Sequence { get; set; }
        public long PrefixId { get; set; }
    }

}