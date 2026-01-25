using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsAvailableLots
    {
        public string LotNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ManufacturingDate { get; set; }
        public int PriceAddedFor { get; set; }
        public DateTime AddedOn { get; set; }
        public string Type { get; set; }
        public decimal QuantityRemaining { get; set; }
        public long Id { get; set; }
        public long AddedBy { get; set; }
        public long CompanyId { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public long ItemDetailsId { get; set; }
        public long BranchId { get; set; }
        public bool IsStopSelling { get; set; }
        public long ItemBranchMapId { get; set; }
        public string Domain { get; set; }
        public long CustomerId { get; set; }
        public long SellingPriceGroupId { get; set; }
    }
}