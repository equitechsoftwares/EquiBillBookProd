using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblPaymentLink")]
    public class ClsPaymentLink
    {
        [Key]
        public long PaymentLinkId { get; set; }
        public long CustomerId { get; set; }
        public DateTime LinkExpirationDate { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public long OnlinePaymentSettingsId { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public long TaxId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public long PrefixId { get; set; }
        public long AccountId { get; set; }
    }

    public class ClsPaymentLinkVm
    {
        public long PaymentLinkId { get; set; }
        public long CustomerId { get; set; }
        public DateTime LinkExpirationDate { get; set; }
        public decimal Amount { get; set; }
        public string Notes { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long BranchId { get; set; }
        public string ReferenceNo { get; set; }
        public string ReferenceId { get; set; }
        public string Status { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string UserType { get; set; }
        public string Domain { get; set; }
        public string CustomerName { get; set; }
        public string CustomerMobileNo { get; set; }
        public long OnlinePaymentSettingsId { get; set; }
        public long PlaceOfSupplyId { get; set; }
        public long TaxId { get; set; }
        public int IsBusinessRegistered { get; set; }
        public string GstTreatment { get; set; }
        public long BusinessRegistrationNameId { get; set; }
        public string BusinessRegistrationNo { get; set; }
        public string BusinessLegalName { get; set; }
        public string BusinessTradeName { get; set; }
        public string PanNo { get; set; }
        public long PrefixId { get; set; }
        public long AccountId { get; set; }
    }

}