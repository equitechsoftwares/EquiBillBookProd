using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantBookingTimeSlot")]
    public class ClsRestaurantBookingTimeSlot
    {
        [Key]
        public long BookingTimeSlotId { get; set; }
        
        public long RestaurantSettingsId { get; set; }
        
        public TimeSpan TimeSlot { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public long AddedBy { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public long ModifiedBy { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        
        public long CompanyId { get; set; }
    }

    public class ClsRestaurantBookingTimeSlotVm
    {
        public long BookingTimeSlotId { get; set; }
        
        public long RestaurantSettingsId { get; set; }
        
        public TimeSpan TimeSlot { get; set; }
        
        // String property for JSON deserialization (will be converted to TimeSpan in controller)
        public string TimeSlotString { get; set; }
        
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

