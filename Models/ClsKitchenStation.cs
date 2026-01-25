using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblKitchenStation")]
    public class ClsKitchenStation
    {
        [Key]
        public long KitchenStationId { get; set; }
        public string StationName { get; set; }
        public string StationType { get; set; }
        public long? PrinterId { get; set; }
        public string PrinterName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsKitchenStationVm
    {
        public long KitchenStationId { get; set; }
        public string StationName { get; set; }
        public string StationType { get; set; }
        public long? PrinterId { get; set; }
        public string PrinterName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        
        // Category mappings - stored in ClsKitchenStationCategoryMap table
        public List<long> CategoryIdList { get; set; }
        
        // Search and pagination
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        
        // Additional properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }
}
