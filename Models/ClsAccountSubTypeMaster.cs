using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblAccountSubTypeMaster")]
    public class ClsAccountSubTypeMaster
    {
        [Key]
        public long AccountSubTypeMasterId { get; set; }
        public long AccountTypeId { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long ParentId { get; set; }
        public string DisplayAs { get; set; }
        public string Type { get; set; }
        public bool CanDelete { get; set; }
        public int Sequence { get; set; }
    }

    public class ClsAccountSubTypeMasterVm
    {
        public long AccountSubTypeMasterId { get; set; }
        public long AccountTypeId { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long ParentId { get; set; }
        public string DisplayAs { get; set; }
        public string Type { get; set; }
        public bool CanDelete { get; set; }
        public int Sequence { get; set; }
    }

}