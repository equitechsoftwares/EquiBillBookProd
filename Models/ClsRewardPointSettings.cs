using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EquiBillBook.Models
{
    [Table("public.tblRewardPointSettings")]
    public class ClsRewardPointSettings
    {
        [Key]
        public long RewardPointSettingsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool EnableRewardPoint { get; set; }
        public string DisplayName { get; set; }
        public decimal AmountSpentForUnitPoint { get; set; }
        public decimal MinOrderTotalToEarnReward { get; set; }
        public decimal MaxPointsPerOrder { get; set; }
        public decimal RedeemAmountPerUnitPoint { get; set; }
        public decimal MinimumOrderTotalToRedeemPoints { get; set; }
        public decimal MinimumRedeemPoint { get; set; }
        public decimal MaximumRedeemPointPerOrder { get; set; }
        public int ExpiryPeriod { get; set; }
        public int ExpiryPeriodType { get; set; }
    }

    public class ClsRewardPointSettingsVm
    {
        public long RewardPointSettingsId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public long CompanyId { get; set; }
        public bool EnableRewardPoint { get; set; }
        public string DisplayName { get; set; }
        public decimal AmountSpentForUnitPoint { get; set; }
        public decimal MinOrderTotalToEarnReward { get; set; }
        public decimal MaxPointsPerOrder { get; set; }
        public decimal RedeemAmountPerUnitPoint { get; set; }
        public decimal MinimumOrderTotalToRedeemPoints { get; set; }
        public decimal MinimumRedeemPoint { get; set; }
        public decimal MaximumRedeemPointPerOrder { get; set; }
        public int ExpiryPeriod { get; set; }
        public int ExpiryPeriodType { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
        public decimal RedeemPoints { get; set; }
        public decimal OrderAmount { get; set; }
        public List<ClsRewardPointTierVm> RewardPointTiers { get; set; }
    }

}