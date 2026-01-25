using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantFloor")]
    public class ClsRestaurantFloor
    {
        [Key]
        public long FloorId { get; set; }
        public string FloorName { get; set; }
        public int FloorNumber { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsRestaurantFloorVm
    {
        public long FloorId { get; set; }
        public string FloorName { get; set; }
        public int FloorNumber { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string BranchName { get; set; }
        
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


