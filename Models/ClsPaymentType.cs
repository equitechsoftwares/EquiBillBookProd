using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPaymentType")]
    public class ClsPaymentType
    {
        [Key]
        public long PaymentTypeId { get; set; }
        public string PaymentType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        //public bool IsPosShown { get; set; }
        public string ShortCutKey { get; set; }
        public bool IsAdvance { get; set; }
        public bool IsOnlyView { get; set; }
        public bool IsPaymentGateway { get; set; }
    }

    public class ClsPaymentTypeVm
    {
        public long PaymentTypeId { get; set; }
        public string PaymentType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string Search { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public long BranchPaymentTypeMapId { get; set; }
        public long BranchId { get; set; }
        public bool IsPosShown { get; set; }
        public decimal Amount { get; set; }
        public string ShortCutKey { get; set; }
        public bool IsAdvance { get; set; }
        public long AccountId { get; set; }
        public bool IsChecked { get; set; }
        public bool IsOnlyView { get; set; }
        public string Domain { get; set; }
        public bool IsPaymentGateway { get; set; }
        public bool IsDefault { get; set; }
    }

}