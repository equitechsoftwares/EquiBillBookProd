using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblResellerPaymentMethod")]
    public class ClsResellerPaymentMethod
    {
        [Key]
        public long ResellerPaymentMethodId { get; set; }
        public int PaymentMethod { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
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
        public string IFSC { get; set; }
        public string SwiftBIC { get; set; }
        public string UpiID { get; set; }
        public string Gpay { get; set; }
        public string PhonePe { get; set; }
        public string Paytm { get; set; }
        public string Paypal { get; set; }
        public string AccountHolder { get; set; }
        public string BankAddress { get; set; }
    }

    public class ClsResellerPaymentMethodVm
    {
        public long ResellerPaymentMethodId { get; set; }
        public int PaymentMethod { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
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
        public string IFSC { get; set; }
        public string SwiftBIC { get; set; }
        public string UpiID { get; set; }
        public string Gpay { get; set; }
        public string PhonePe { get; set; }
        public string Paytm { get; set; }
        public string Paypal { get; set; }
        public string AccountHolder { get; set; }
        public string BankAddress { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public long UserId { get; set; }
    }

}