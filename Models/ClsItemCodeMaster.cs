using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblItemCodeMaster")]
    public class ClsItemCodeMaster
    {
        [Key]
        public long ItemCodeMasterId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string ItemCodeType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class ClsItemCodeMasterVm
    {
        public long ItemCodeMasterId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string ItemCodeType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Search { get; set; }
        public List<ClsItemCodeMasterImport> ItemCodeMasterImports { get; set; }
    }

    public class ClsItemCodeMasterImport
    {
        public string ItemCodeType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
