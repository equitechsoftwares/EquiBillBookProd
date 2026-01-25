using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPaymentTerm")]
    public class ClsPaymentTerm
    {
        [Key]
        public long PaymentTermId { get; set; }
        public string PaymentTerm { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsDueUponReceipt { get; set; }
    }

    public class ClsPaymentTermVm
    {
        public long PaymentTermId { get; set; }
        public string PaymentTerm { get; set; }
        public int Days { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public string Domain { get; set; }
        public bool IsDueUponReceipt { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
    }

}