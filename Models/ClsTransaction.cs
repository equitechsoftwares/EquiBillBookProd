using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblTransaction")]
    public class ClsTransaction
    {
        [Key]
        public long TransactionId { get; set; }
        public long ParentTransactionId { get; set; }
        public string TransactionNo { get; set; }
        public long PlanId { get; set; }
        public long TermLengthId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal YearlyPlanDiscount { get; set; }
        public decimal YearlyPlanDiscountPercentage { get; set; }
        public decimal PayableCost { get; set; }
        public int Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsTrial { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public int BaseBranch { get; set; }
        public int BaseUser { get; set; }
        public int BaseOrder { get; set; }
        public int BaseItem { get; set; }
        public int BaseBill { get; set; }
        public int BaseTaxSetting { get; set; }
        public int BaseDomain { get; set; }
        public DateTime? PaidOn { get; set; }
        public string LiveTransactionId { get; set; }
        public string PaymentGatewayType { get; set; }
        public string PaymentMethodType { get; set; }
        public long CouponId { get; set; }
        public decimal RootAccountCommissionPercent { get; set; }
        public decimal WhitelabelCommissionPercent { get; set; }
        public decimal ResellerCommissionPercent { get; set; }
        public decimal SpecialDiscount { get; set; }
        public  long CurrencyId { get; set; }
        public int Months { get; set; }
        public decimal NominalSubTotal { get; set; }
        public decimal RootAccountCommission { get; set; }
        public decimal WhitelabelCommission { get; set; }
        public decimal ResellerCommission { get; set; }
    }

    public class ClsTransactionVm
    {
        public long TransactionId { get; set; }
        public long ParentTransactionId { get; set; }
        public string TransactionNo { get; set; }
        public long PlanId { get; set; }
        public long TermLengthId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal YearlyPlanDiscount { get; set; }
        public decimal YearlyPlanDiscountPercentage { get; set; }
        public decimal PayableCost { get; set; }
        public int Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsTrial { get; set; }
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
        public IList<ClsTransactionDetailsVm> TransactionDetails { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string AddedByCode { get; set; }
        public string ModifiedByCode { get; set; }
        public int BaseBranch { get; set; }
        public int BaseUser { get; set; }
        public int BaseOrder { get; set; }
        public int BaseItem { get; set; }
        public int BaseBill { get; set; }
        public int BaseTaxSetting { get; set; }
        public int BaseDomain { get; set; }
        public ClsBusinessSettingsVm BusinessSetting { get; set; }
        public ClsBranchVm Branch { get; set; }
        public DateTime? PaidOn { get; set; }
        public string LiveTransactionId { get; set; }
        public string PaymentGatewayType { get; set; }
        public string PaymentMethodType { get; set; }
        public string Name { get; set; }
        public string EmailId { get; set; }
        public string MobileNo { get; set; }
        public int DaysLeft { get; set; }
        public long CurrentPlanId { get; set; }
        public long CountryId { get; set; }
        public string WarningMsg { get; set; }
        public int MonthsLeft { get; set; }
        public long CouponId { get; set; }
        public string Domain { get; set; }
        public long Under { get; set; }
        public long UserId { get; set; }
        public List<ClsOnlinePaymentSettingsVm> OnlinePaymentSettings { get; set; }
        public long MainCountryId { get; set; }
        public string BusinessLogo { get; set; }
        public string BusinessName { get; set; }
        public ClsBusinessSettingsVm From { get; set; }
        public decimal CommissionPercent { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long TotalNoOfSales { get; set; }
        public decimal TotalSales { get; set; }
        public string CurrencySymbol { get; set; }
        public bool IsPaid { get; set; }
        public decimal RootAccountCommissionPercent { get; set; }
        public decimal WhitelabelCommissionPercent { get; set; }
        public decimal ResellerCommissionPercent { get; set; }
        public decimal SpecialDiscount { get; set; }
        public decimal MinPrice { get; set; }
        public long CurrencyId { get; set; }
        public decimal NominalSubTotal { get; set; }
        public int CurrencySymbolPlacement { get; set; }
        public decimal RootAccountCommission{ get; set; }
        public decimal WhitelabelCommission{ get; set; }
        public decimal ResellerCommission{ get; set; }
        public decimal PayableCommission { get; set; }
        public string Title { get; set; }
        public int Months { get; set; }
        public string TransactionType { get; set; }
        public decimal Points { get; set; }
        public string InvoiceNo { get; set; }
        public decimal OrderAmount { get; set; }
        public string Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public string CustomerName { get; set; }
        public bool IsExpired { get; set; }
    }
}