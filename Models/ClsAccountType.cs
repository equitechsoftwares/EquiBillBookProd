using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblAccountType")]
    public class ClsAccountType
    {
        [Key]
        public long AccountTypeId { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountType { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
    }

    public class ClsAccountTypeVm
    {
        public long AccountTypeId { get; set; }
        public string AccountTypeCode { get; set; }
        public string AccountType { get; set; }
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
        public string Domain { get; set; }
        public List<ClsAccountSubHeadersVm> AccountSubHeaders { get; set; }
        //public ClsAccountSubHeadersVm AccountSubHeader { get; set; }
        public List<ClsAccountSubTypeVm> AccountSubTypes { get; set; }
        public List<ClsAccountVm> Accounts { get; set; }
        public List<ClsAccountOpeningBalanceVm> AccountOpeningBalances { get; set; }
        public DateTime? MigrationDate { get; set; }
    }

    public class ClsAccountSubHeadersVm
    {
        public string AccountSubHeader { get; set; }
        public List<ClsAccountSubTypeVm> AccountSubTypes { get; set; }
    }

}