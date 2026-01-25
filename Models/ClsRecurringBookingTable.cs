using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRecurringBookingTable")]
    public class ClsRecurringBookingTable
    {
        [Key]
        public long RecurringBookingTableId { get; set; }
        
        public long RecurringBookingId { get; set; }
        
        public long TableId { get; set; }
        
        public bool IsPrimary { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public DateTime AddedOn { get; set; }
    }
}

