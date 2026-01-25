using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblKotDetails")]
    public class ClsKotDetails
    {
        [Key]
        public long KotDetailsId { get; set; }
        public long KotId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public decimal Quantity { get; set; }
        public long UnitId { get; set; }
        public string CookingInstructions { get; set; }
        public string ItemStatus { get; set; }
        public long KitchenStationId { get; set; }
        public int EstimatedTime { get; set; }
        public DateTime? StartedCookingAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? ServedAt { get; set; }
        public int Priority { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsKotDetailsVm
    {
        public long KotDetailsId { get; set; }
        public long KotId { get; set; }
        public long ItemId { get; set; }
        public long ItemDetailsId { get; set; }
        public string ItemName { get; set; }  // Computed from ItemId/ItemDetailsId for display purposes
        public decimal Quantity { get; set; }
        public long UnitId { get; set; }
        public string Unit { get; set; }  // Computed from UnitId for display purposes
        public string CookingInstructions { get; set; }
        public string ItemStatus { get; set; }
        public long KitchenStationId { get; set; }
        public int EstimatedTime { get; set; }
        public DateTime? StartedCookingAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? ServedAt { get; set; }
        public int Priority { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
        // Additional display properties
        public string KitchenStationName { get; set; }
        public string ItemCode { get; set; }
        
        // Additional properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
    }
}


