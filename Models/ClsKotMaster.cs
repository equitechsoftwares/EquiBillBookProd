using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblKotMaster")]
    public class ClsKotMaster
    {
        [Key]
        public long KotId { get; set; }
        public string KotNo { get; set; }
        public long TableId { get; set; }
        public long SalesId { get; set; }
        public long BookingId { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? ExpectedTime { get; set; }
        public DateTime? ReadyTime { get; set; }
        public DateTime? ServedTime { get; set; }
        public long WaiterId { get; set; }
        public int GuestCount { get; set; }
        public long CustomerId { get; set; }
        public string SpecialInstructions { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool Printed { get; set; }
        public DateTime? PrintedOn { get; set; }
        public long PrintedBy { get; set; }
        
        // Navigation property
        public virtual ICollection<ClsKotDetails> KotDetails { get; set; }
    }

    public class ClsKotMasterVm
    {
        public long KotId { get; set; }
        public string KotNo { get; set; }
        public long TableId { get; set; }
        public long SalesId { get; set; }
        public long BookingId { get; set; }
        public string OrderType { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderTime { get; set; }
        public DateTime? ExpectedTime { get; set; }
        public DateTime? ReadyTime { get; set; }
        public DateTime? ServedTime { get; set; }
        public long WaiterId { get; set; }
        public int GuestCount { get; set; }
        public long CustomerId { get; set; }
        public string SpecialInstructions { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool Printed { get; set; }
        public DateTime? PrintedOn { get; set; }
        public long PrintedBy { get; set; }
        
        // Additional display properties
        public string TableNo { get; set; }
        public string TableName { get; set; }
        public string WaiterName { get; set; }
        public string CustomerName { get; set; }
        public string SalesNo { get; set; }
        public string BookingNo { get; set; }
        
        // KOT Details list
        public List<ClsKotDetailsVm> KotDetails { get; set; }
        
        // Search and pagination
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool WithSales { get; set; }
        
        // Additional properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public long KitchenStationId { get; set; }
        public string UserType { get; set; }
        
        // For bulk operations (converting multiple KOTs to sales)
        public List<long> KotIds { get; set; }
    }
}


