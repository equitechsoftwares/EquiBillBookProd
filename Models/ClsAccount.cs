using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblAccount")]
    public class ClsAccount
    {
        [Key]
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public long AccountTypeId { get; set; }
        public long AccountSubTypeId { get; set; }
        //public decimal OpeningBalance { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public int Status { get; set; }
        public string Type { get; set; }
        public long CurrencyId { get; set; }
        public bool CanDelete { get; set; }
        public long ParentId { get; set; }
        public string DisplayAs { get; set; }
    }

    public class ClsAccountVm
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public long AccountTypeId { get; set; }
        public long AccountSubTypeId { get; set; }
        //public decimal OpeningBalance { get; set; }
        public string Notes { get; set; }
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
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public List<ClsAccountDetailsVm> AccountDetails { get; set; }
        public long BranchId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public int Status { get; set; }
        public string PaymentType { get; set; }
        public string AccountType { get; set; }
        public string AccountSubType { get; set; }
        public string Domain { get; set; }
        public string Type { get; set; }
        public long CurrencyId { get; set; }
        public bool CanDelete { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public long AccountOpeningBalanceId { get; set; }
        public decimal OpeningBalanceDebit { get; set; }
        public decimal OpeningBalanceCredit { get; set; }
        public long ParentId { get; set; }
        public string DisplayAs { get; set; }
        public string Page { get; set; }
        public string SubPage { get; set; }
        public string Unit { get; set; }
        public string Code { get; set; }
        public decimal TaxPercent { get; set; }
        public long TaxSettingId { get; set; }
    }

}