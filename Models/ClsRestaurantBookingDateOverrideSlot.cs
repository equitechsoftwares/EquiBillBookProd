using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantBookingDateOverrideSlot")]
    public class ClsRestaurantBookingDateOverrideSlot
    {
        [Key]
        public long OverrideSlotId { get; set; }
        
        public long BookingDateOverrideId { get; set; }
        
        public TimeSpan TimeSlot { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public long AddedBy { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public long ModifiedBy { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        
        public long CompanyId { get; set; }
    }

    public class ClsRestaurantBookingDateOverrideSlotVm
    {
        public long OverrideSlotId { get; set; }
        
        public long BookingDateOverrideId { get; set; }
        
        public TimeSpan TimeSlot { get; set; }
        
        // String property for JSON deserialization (will be converted to TimeSpan in controller)
        public string TimeSlotString { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public long AddedBy { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public long ModifiedBy { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        
        public long CompanyId { get; set; }
        
        // Search and pagination
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        
        // Additional properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
    }
}