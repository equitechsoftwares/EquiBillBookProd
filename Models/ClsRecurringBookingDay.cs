using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRecurringBookingDay")]
    public class ClsRecurringBookingDay
    {
        [Key]
        public long RecurringBookingDayId { get; set; }
        
        public long RecurringBookingId { get; set; }
        
        /// <summary>
        /// Day of week: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
        /// </summary>
        public int DayOfWeek { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public DateTime AddedOn { get; set; }
    }
}

