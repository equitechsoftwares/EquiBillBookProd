using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblResellerPayment")]
    public class ClsResellerPayment
    {
        [Key]
        public long ResellerPaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PayableAmount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string ReferenceNo { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Type { get; set; }
        public bool IsDebit { get; set; }
    }

    public class ClsResellerPaymentVm
    {
        public long ResellerPaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public decimal PayableAmount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string AttachDocument { get; set; }
        public string ReferenceNo { get; set; }
        public long UserId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Type { get; set; }
        public bool IsDebit { get; set; }
    }

}