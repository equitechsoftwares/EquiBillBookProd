using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantBookingDateOverride")]
    public class ClsRestaurantBookingDateOverride
    {
        [Key]
        public long BookingDateOverrideId { get; set; }
        
        public long RestaurantSettingsId { get; set; }
        
        public DateTime OverrideDate { get; set; }
        
        public bool IsClosed { get; set; }
        
        public string Reason { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public long AddedBy { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public long ModifiedBy { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        
        public long CompanyId { get; set; }
    }

    public class ClsRestaurantBookingDateOverrideVm
    {
        public long BookingDateOverrideId { get; set; }
        
        public long RestaurantSettingsId { get; set; }
        
        public DateTime OverrideDate { get; set; }
        
        // String property for JSON deserialization (will be converted to DateTime in controller)
        public string OverrideDateString { get; set; }
        
        public bool IsClosed { get; set; }
        
        public string Reason { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public long AddedBy { get; set; }
        
        public DateTime AddedOn { get; set; }
        
        public long ModifiedBy { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        
        public long CompanyId { get; set; }
        
        public List<ClsRestaurantBookingDateOverrideSlotVm> TimeSlots { get; set; }
        
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