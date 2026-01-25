using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    public class ClsMyPlan
    {
        public int TotalUser { get; set; }
        public int TotalBranch { get; set; }
        public int TotalItem { get; set; }
        public int TotalOrder { get; set; }
        public int TotalDomain { get; set; }
        public int TotalCatalogue { get; set; }
        public int TotalSms { get; set; }
        public int TotalEmail{ get; set; }
        public int TotalBill { get; set; }
        public int TotalTaxSetting { get; set; }
        public int TotalUserUsed { get; set; }
        public int TotalBranchUsed { get; set; }
        public int TotalItemUsed { get; set; }
        public int TotalOrderUsed { get; set; }
        public int TotalDomainUsed { get; set; }
        public int TotalCatalogueUsed { get; set; }
        public int TotalSmsUsed { get; set; }
        public int TotalEmailUsed { get; set; }
        public int TotalBillUsed { get; set; }
        public int TotalTaxSettingUsed { get; set; }
        public decimal UserPercentageRemaining { get; set; }
        public decimal BranchPercentageRemaining { get; set; }
        public decimal ItemPercentageRemaining { get; set; }
        public decimal OrderPercentageRemaining { get; set; }
        public decimal DomainPercentageRemaining { get; set; }
        public decimal CataloguePercentageRemaining { get; set; }
        public decimal SmsPercentageRemaining { get; set; }
        public decimal EmailPercentageRemaining { get; set; }
        public decimal BillPercentageRemaining { get; set; }
        public decimal TaxSettingPercentageRemaining { get; set; }
        public ClsTransactionVm Transaction { get; set; }
        public IList<ClsTransactionVm> UnusedPlans { get; set; }
        public IList<ClsPlanAddonsVm> PlanAddons { get; set; }
    }
}