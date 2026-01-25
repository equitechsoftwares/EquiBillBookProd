using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblBranch")]
    public class ClsBranch
    {
        [Key]
        public long BranchId { get; set; }
        public string BranchCode { get; set; }
        public string Branch { get; set; }
        public long ContactPersonId { get; set; }
        public string ContactName { get; set; }
        public string Mobile { get; set; }
        public string AltMobileNo { get; set; }
        public string Email { get; set; }
        public long CityId { get; set; }
        public long StateId { get; set; }
        public long CountryId { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool IsMain { get; set; }
        public long TaxId { get; set; }
        public string TaxNo { get; set; }
        public bool EnableInlineTax { get; set; }
        public long TaxSettingId { get; set; }
        public long PrefixId { get; set; }
        public string WhatsappNo { get; set; }
    }
    public class ClsBranchVm
    {
        public long BranchId { get; set; }
        public string BranchCode { get; set; }
        public string Branch { get; set; }
        public long ContactPersonId { get; set; }
        public string ContactName { get; set; }
        public string Mobile { get; set; }
        public string AltMobileNo { get; set; }
        public string Email { get; set; }
        public long CityId { get; set; }
        public long StateId { get; set; }
        public long CountryId { get; set; }
        public string Address { get; set; }
        public string Zipcode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string Search { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Title { get; set; }
        public bool IsMain { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public List<ClsPaymentTypeVm> PaymentTypes { get; set; }
        public string PaymentTypesIds { get; set; }
        public long TaxId { get; set; }
        public string TaxNo { get; set; }
        public bool EnableInlineTax { get; set; }
        public string UserType { get; set; }
        public string Rack { get; set; }
        public string Row { get; set; }
        public string Position { get; set; }
        public bool IsChecked { get; set; }
        public string Tax { get; set; }
        public string Name { get; set; }
        public string PaymentType { get; set; }
        public string Domain { get; set; }
        public long Under { get; set; }
        public decimal Quantity { get; set; }
        public long TaxSettingId { get; set; }
        public long PrefixId { get; set; }
        public string Type { get; set; }
        public string ItemCodeType { get; set; }
        public string TaxExemptionType { get; set; }
        public string WhatsappNo { get; set; }
        public string DialingCode { get; set; }
    }
}