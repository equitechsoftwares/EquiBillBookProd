using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblBranchPaymentTypeMap")]
    public class ClsBranchPaymentTypeMap
    {
        [Key]
        public long BranchPaymentTypeMapId { get; set; }
        public long BranchId { get; set; }
        public long PaymentTypeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long AccountId { get; set; }
        public bool IsPosShown { get; set; }
        public bool IsDefault { get; set; }
    }

    public class ClsBranchPaymentTypeMapVm
    {
        public long BranchPaymentTypeMapId { get; set; }
        public long BranchId { get; set; }
        public long PaymentTypeId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long AccountId { get; set; }
        public bool IsPosShown { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public bool IsDefault { get; set; }
    }

}