using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRecurringBooking")]
    public class ClsRecurringBooking
    {
        [Key]
        public long RecurringBookingId { get; set; }
        public long BookingId { get; set; } // Use 0 instead of null - can create recurring booking without template
        
        // Booking detail fields (used when BookingId is 0)
        public long CustomerId { get; set; }
        public TimeSpan? BookingTime { get; set; }
        public int Duration { get; set; }
        public int NoOfGuests { get; set; }
        public long BranchId { get; set; }
        public long FloorId { get; set; }
        public string SpecialRequest { get; set; }
        
        public string RecurrenceType { get; set; }
        public int RepeatEveryNumber { get; set; }
        public string RepeatEvery { get; set; }
        public int? DayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsNeverExpires { get; set; }
        public bool IsActive { get; set; }
        public long CompanyId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
    }

    public class ClsRecurringBookingVm
    {
        public long RecurringBookingId { get; set; }
        public long BookingId { get; set; } // Use 0 instead of null
        public string BookingNo { get; set; }
        
        // Booking detail fields (used when BookingId is 0)
        public long CustomerId { get; set; }
        public string BookingTimeString { get; set; }
        public TimeSpan? BookingTime { get; set; }
        public int Duration { get; set; }
        public int NoOfGuests { get; set; }
        public long BranchId { get; set; }
        public long FloorId { get; set; }
        public List<long> TableIds { get; set; }
        public string SpecialRequest { get; set; }        
        public string RecurrenceType { get; set; }
        public int RepeatEveryNumber { get; set; }
        public string RepeatEvery { get; set; }
        public int? DayOfMonth { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsNeverExpires { get; set; }
        public bool IsActive { get; set; }
        public long CompanyId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public List<int> DaysOfWeek { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string SearchText { get; set; }
        public string Search { get; set; } // Alias for SearchText to match Category pattern
    }
}


