using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTransactionDetails")]
    public class ClsTransactionDetails
    {
        [Key]
        public long TransactionDetailsId { get; set; }
        public long TransactionId { get; set; }
        public long PlanAddonsId { get; set; }
        public decimal MRP { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Type { get; set; }
    }

    public class ClsTransactionDetailsVm
    {
        public long TransactionDetailsId { get; set; }
        public long TransactionId { get; set; }
        public long PlanAddonsId { get; set; }
        public decimal MRP { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string PricingPer { get; set; }
        public int PricingType { get; set; }
        public bool IsCheckbox { get; set; }
    }

}