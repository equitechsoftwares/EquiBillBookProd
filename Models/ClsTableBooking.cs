using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblTableBooking")]
    public class ClsTableBooking
    {
        [Key]
        public long BookingId { get; set; }
        public string BookingNo { get; set; }
        public long CustomerId { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        public int Duration { get; set; }
        public int NoOfGuests { get; set; }
        public string Status { get; set; }
        public string BookingType { get; set; }
        public string SpecialRequest { get; set; }
        public decimal DepositAmount { get; set; }
        public bool ReminderSent { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? ConfirmedOn { get; set; }
        public DateTime? CancelledOn { get; set; }
        public string CancellationReason { get; set; }
        public decimal CancellationCharge { get; set; }
        public long SalesId { get; set; }
        public long WaiterId { get; set; }
        public string PublicBookingToken { get; set; }
        public string IpAddress { get; set; }
        public string PaymentTransactionId { get; set; }
        public string PaymentGatewayType { get; set; }
        public DateTime? PaymentDate { get; set; }
        public long RecurringBookingId { get; set; }
    }

    public class ClsTableBookingVm
    {
        public long BookingId { get; set; }
        public string BookingNo { get; set; }
        public List<long> TableIds { get; set; } = new List<long>(); // Support multiple tables
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobile { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime BookingDate { get; set; }
        public TimeSpan BookingTime { get; set; }
        // String property for JSON deserialization (will be converted to TimeSpan in controller)
        public string BookingTimeString { get; set; }
        public int Duration { get; set; }
        public int NoOfGuests { get; set; }
        public string Status { get; set; }
        public string BookingType { get; set; }
        public string SpecialRequest { get; set; }
        public decimal DepositAmount { get; set; }
        public bool ReminderSent { get; set; }
        public long BranchId { get; set; }
        public long FloorId { get; set; } // Floor ID from the first table (for pre-selecting floor in edit form)
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public DateTime? ConfirmedOn { get; set; }
        public DateTime? CancelledOn { get; set; }
        public string CancellationReason { get; set; }
        public decimal CancellationCharge { get; set; }
        public long SalesId { get; set; }
        public long WaiterId { get; set; }
        public string WaiterName { get; set; } // Display property - retrieved via WaiterId join to user table
        public string SalesNo { get; set; } // Sales invoice number (fetched from Sales table)
        public long KotId { get; set; } // Used for API calls (LinkToKot), not stored in database
        public List<string> KotNos { get; set; } = new List<string>(); // List of KOT numbers associated with this booking
        public string KotNo => KotNos != null && KotNos.Count > 0 ? string.Join(", ", KotNos) : null; // Display property - comma-separated KOT numbers
        public string PublicBookingToken { get; set; }
        public string PaymentTransactionId { get; set; }
        public string PaymentGatewayType { get; set; }
        public DateTime? PaymentDate { get; set; }
        
        // Calendar properties for FullCalendar
        public string Title { get; set; }
        public string Color { get; set; }
        public bool AllDay { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        
        // Additional display properties
        public string TableNo { get; set; }
        public string TableName { get; set; }
        public List<string> TableNos { get; set; } = new List<string>(); // Multiple table numbers
        public List<string> TableNames { get; set; } = new List<string>(); // Multiple table names
        public string CustomerNameDisplay { get; set; }
        
        // Search and pagination
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        
        // Additional properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string UserType { get; set; }
        
        // Payment summary properties
        public decimal TotalPaid { get; set; }
        public decimal? TotalRefunded { get; set; }
        public decimal NetPaid { get; set; }
        public decimal DueAmount { get; set; }
        public decimal AdvanceBalance { get; set; }
        
        // Recurring booking properties
        public long RecurringBookingId { get; set; }
        public string RecurrenceType { get; set; }
        public int RepeatEveryNumber { get; set; }
        public int? DayOfMonth { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long id { get; set; }
    }
}


