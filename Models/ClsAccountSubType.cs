using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblAccountSubType")]
    public class ClsAccountSubType
    {
        [Key]
        public long AccountSubTypeId { get; set; }
        public long AccountTypeId { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public long ParentId { get; set; }
        public string DisplayAs { get; set; }
        public string Type { get; set; }
        public bool CanDelete { get; set; }
    }

    public class ClsAccountSubTypeVm
    {
        public long AccountSubTypeId { get; set; }
        public long AccountTypeId { get; set; }
        public string AccountSubTypeCode { get; set; }
        public string AccountSubType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public string Search { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Title { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public string AccountType { get; set; }
        public string Domain { get; set; }
        public long ParentId { get; set; }
        public string DisplayAs { get; set; }
        public string Type { get; set; }
        public List<ClsAccountVm> Accounts { get; set; }
        public bool CanDelete { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal OpeningBalanceDebit { get; set; }
        public decimal OpeningBalanceCredit { get; set; }
        public int Sequence { get; set; }
    }

}