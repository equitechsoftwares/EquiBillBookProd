using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblTableBookingTable")]
    public class ClsTableBookingTable
    {
        [Key]
        public long BookingTableId { get; set; }
        
        public long BookingId { get; set; }
        
        public long TableId { get; set; }
        
        public bool IsPrimary { get; set; }
        
        public int DisplayOrder { get; set; }
        
        public DateTime AddedOn { get; set; }
        
    }
}

