using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblItemDetailsVariationMap")]
    public class ClsItemDetailsVariationMap
    {
        [Key]
        public long ItemDetailsVariationMapId { get; set; }
        public long ItemDetailsId { get; set; }
        public long VariationId { get; set; }
        public long VariationDetailsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsItemDetailsVariationMapVm
    {
        public long ItemDetailsVariationMapId { get; set; }
        public long ItemDetailsId { get; set; }
        public long VariationId { get; set; }
        public long VariationDetailsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string VariationName { get; set; }
        public string VariationDetailsName { get; set; }
    }
}

