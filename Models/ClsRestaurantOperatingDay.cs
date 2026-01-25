using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantOperatingDay")]
    public class ClsRestaurantOperatingDay
    {
        [Key]
        public long RestaurantOperatingDayId { get; set; }
        
        public long RestaurantSettingsId { get; set; }
        
        /// <summary>
        /// Day of week: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
        /// </summary>
        public int DayOfWeek { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public long AddedBy { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public long ModifiedBy { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        
        public long CompanyId { get; set; }
    }
}

