using System;

namespace EquiBillBook.Models
{
    public class ClsBookingStatsVm
    {
        public int TotalTodayBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int ActiveBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public int ActiveRecurringBookings { get; set; }
        public decimal BookingConversionRate { get; set; }
        public double AverageBookingDuration { get; set; }
    }

    public class ClsKotStatsVm
    {
        public int TotalTodayKots { get; set; }
        public int PendingKots { get; set; }
        public int PreparingKots { get; set; }
        public int ReadyKots { get; set; }
        public int CompletedKots { get; set; }
    }

    public class ClsTableStatsVm
    {
        public int TotalTables { get; set; }
        public int AvailableTables { get; set; }
        public int OccupiedTables { get; set; }
        public int ReservedTables { get; set; }
        public int BookedTables { get; set; }
        public int MaintenanceTables { get; set; }
    }

    public class ClsRevenueStatsVm
    {
        public decimal TodayRevenue { get; set; }
    }
}

