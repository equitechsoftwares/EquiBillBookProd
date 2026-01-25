using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblShortCutKeySetting")]
    public class ClsShortCutKeySetting
    {
        [Key]
        public long ShortCutKeySettingId { get; set; }
        public long MenuId { get; set; }
        public string Title { get; set; }
        public string ShortCutKey { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsShortCutKeySettingVm
    {
        public long ShortCutKeySettingId { get; set; }
        public long MenuId { get; set; }
        public string Title { get; set; }
        public string ShortCutKey { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Menu { get; set; }
        public string Url { get; set; }
        public bool IsView { get; set; }
        public long divId { get; set; }
        public string Domain { get; set; }
    }

}