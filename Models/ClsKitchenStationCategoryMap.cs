using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblKitchenStationCategoryMap")]
    public class ClsKitchenStationCategoryMap
    {
        [Key]
        public long MappingId { get; set; }
        public long KitchenStationId { get; set; }
        public long CategoryId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
    }
}


