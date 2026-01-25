using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblCashRegister")]
    public class ClsCashRegister
    {
        [Key]
        public long CashRegisterId { get; set; }
        public long BranchId { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal CashInHand { get; set; }
        public int Status { get; set; }
        public string ClosingNote { get; set; }
    }

    public class ClsCashRegisterVm
    {
        public long CashRegisterId { get; set; }
        public long BranchId { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public decimal CashInHand { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int Status { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ClosingNote { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Branch { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public List<ClsPaymentTypeVm> PaymentTypes { get; set; }
        public long UserId { get; set; }
        public int Type { get; set; }
        public string Domain { get; set; }
        public string BranchName { get; set; }
        public decimal ChangeReturn { get; set; }
        public long StateId { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal TotalCreditSales { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalPayment { get; set; }
    }

}