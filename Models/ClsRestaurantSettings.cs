using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantSettings")]
    public class ClsRestaurantSettings
    {
        [Key]
        public long RestaurantSettingsId { get; set; }

        // Kitchen Display Settings
        public bool EnableKitchenDisplay { get; set; }
        public bool AutoPrintKot { get; set; }

        // Table Booking Settings
        public bool EnableTableBooking { get; set; }
        public bool EnableRecurringBooking { get; set; }
        public int BookingAdvanceDays { get; set; }
        public TimeSpan? BookingStartTime { get; set; }
        public TimeSpan? BookingEndTime { get; set; }
        public string BookingTimeSlotMode { get; set; } // "Auto" or "Manual"
        public int DefaultBookingDuration { get; set; }

        // Deposit Settings
        public bool RequireDeposit { get; set; }
        public string DepositMode { get; set; }
        public decimal DepositFixedAmount { get; set; }
        public decimal DepositPerGuestAmount { get; set; }

        // Public Booking Settings
        public bool EnablePublicBooking { get; set; }
        public string PublicBookingSlug { get; set; }
        public int PublicBookingAdvanceDays { get; set; }
        public bool PublicBookingRequireDeposit { get; set; }
        public string PublicBookingDepositMode { get; set; }
        public decimal PublicBookingDepositPercentage { get; set; }
        public decimal PublicBookingDepositFixedAmount { get; set; }
        public decimal PublicBookingDepositPerGuestAmount { get; set; }
        public bool PublicBookingAutoConfirm { get; set; }

        // Public Booking Cancellation Settings
        public bool EnablePublicBookingCancellation { get; set; }
        public bool AllowCancelAfterConfirm { get; set; }
        public int PublicBookingCancellationDaysBefore { get; set; }
        public string PublicBookingCancellationChargeMode { get; set; }
        public decimal PublicBookingCancellationFixedCharge { get; set; }
        public decimal PublicBookingCancellationPercentage { get; set; }
        public decimal PublicBookingCancellationPerGuestCharge { get; set; }

        // QR Code
        public string QRCodeImage { get; set; }

        // System Fields
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsRestaurantSettingsVm
    {
        public long RestaurantSettingsId { get; set; }

        // Kitchen Display Settings
        public bool EnableKitchenDisplay { get; set; }
        public bool AutoPrintKot { get; set; }

        // Table Booking Settings
        public bool EnableTableBooking { get; set; }
        public bool EnableRecurringBooking { get; set; }
        public int BookingAdvanceDays { get; set; }
        public TimeSpan? BookingStartTime { get; set; }
        public TimeSpan? BookingEndTime { get; set; }
        // String properties for JSON deserialization (will be converted to TimeSpan in controller)
        public string BookingStartTimeString { get; set; }
        public string BookingEndTimeString { get; set; }
        public string BookingTimeSlotMode { get; set; } // "Auto" or "Manual"
        public int DefaultBookingDuration { get; set; }

        // Deposit Settings
        public bool RequireDeposit { get; set; }
        public string DepositMode { get; set; }
        public decimal DepositFixedAmount { get; set; }
        public decimal DepositPerGuestAmount { get; set; }

        // Public Booking Settings
        public bool EnablePublicBooking { get; set; }
        public string PublicBookingSlug { get; set; }
        public int PublicBookingAdvanceDays { get; set; }
        public bool PublicBookingRequireDeposit { get; set; }
        public string PublicBookingDepositMode { get; set; }
        public decimal PublicBookingDepositPercentage { get; set; }
        public decimal PublicBookingDepositFixedAmount { get; set; }
        public decimal PublicBookingDepositPerGuestAmount { get; set; }
        public bool PublicBookingAutoConfirm { get; set; }

        // Public Booking Cancellation Settings
        public bool EnablePublicBookingCancellation { get; set; }
        public bool AllowCancelAfterConfirm { get; set; }
        public int PublicBookingCancellationDaysBefore { get; set; }
        public string PublicBookingCancellationChargeMode { get; set; }
        public decimal PublicBookingCancellationFixedCharge { get; set; }
        public decimal PublicBookingCancellationPercentage { get; set; }
        public decimal PublicBookingCancellationPerGuestCharge { get; set; }

        // QR Code
        public string QRCodeImage { get; set; }

        // System Fields
        public long BranchId { get; set; }
        public string Branch { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Normalized Properties
        public List<ClsRestaurantOperatingDay> OperatingDays { get; set; }
        // Alternative property for JSON deserialization when OperatingDays is sent as array of integers
        public List<int> OperatingDaysInt { get; set; }
        public List<ClsRestaurantBookingTimeSlotVm> BookingTimeSlotsNormalized { get; set; }
        // Alternative property for JSON deserialization when BookingTimeSlotsList is sent as array of strings (HH:mm format)
        public List<string> BookingTimeSlotsList { get; set; }
        public List<ClsRestaurantBookingDateOverrideVm> DateOverrides { get; set; }

        // Request Properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }

        // Pagination Properties
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
    }
}
