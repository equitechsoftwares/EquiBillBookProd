using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRestaurantTable")]
    public class ClsRestaurantTable
    {
        [Key]
        public long TableId { get; set; }
        public string TableNo { get; set; }
        public string TableName { get; set; }
        public int Capacity { get; set; }
        public long FloorId { get; set; }
        public long TableTypeId { get; set; }
        public bool IsMaintenanceMode { get; set; }
        public DateTime? MaintenanceFrom { get; set; }
        public DateTime? MaintenanceTo { get; set; }
        public string MaintenanceReason { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        // Stored relative path to QR code image (similar to Catalogue.QRCodeImage)
        public string QRCodeImage { get; set; }
        // Unique URL slug for direct table booking (e.g., table-5, vip-corner)
        public string TableSlug { get; set; }
        // Table status (Available, Occupied, Reserved, Maintenance, Booked)
        public string Status { get; set; }
    }

    public class ClsRestaurantTableVm
    {
        public long TableId { get; set; }
        public string TableNo { get; set; }
        public string TableName { get; set; }
        public int Capacity { get; set; }
        public long FloorId { get; set; }
        public long TableTypeId { get; set; }
        public bool IsMaintenanceMode { get; set; }
        public DateTime? MaintenanceFrom { get; set; }
        public DateTime? MaintenanceTo { get; set; }
        public string MaintenanceReason { get; set; }
        public long BranchId { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string QRCodeImage { get; set; }
        // Unique URL slug for direct table booking (e.g., table-5, vip-corner)
        public string TableSlug { get; set; }
        // Table status (Available, Occupied, Reserved, Maintenance, Booked)
        public string Status { get; set; }
        
        // Calculated properties (not stored in DB)
        public string BranchName { get; set; }
        public string FloorName { get; set; }
        public string TableTypeName { get; set; }
        
        // Search and pagination
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public long TotalCount { get; set; }
        
        // Additional properties
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public DateTime? ForDateTime { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}


