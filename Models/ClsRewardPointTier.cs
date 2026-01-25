using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EquiBillBook.Models
{
    [Table("public.tblRewardPointTiers")]
    public class ClsRewardPointTier
    {
        [Key]
        public long RewardPointTierId { get; set; }
        public long RewardPointSettingsId { get; set; }
        public long CompanyId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal? MaxAmount { get; set; } // NULL means no upper limit
        public decimal AmountSpentForUnitPoint { get; set; } // Amount needed to earn 1 point in this tier
        public int Priority { get; set; } // Lower number = higher priority (checked first)
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }

    public class ClsRewardPointTierVm
    {
        public long RewardPointTierId { get; set; }
        public long RewardPointSettingsId { get; set; }
        public long CompanyId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal AmountSpentForUnitPoint { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedOn { get; set; }
        public long ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string Browser { get; set; }
        public string IpAddress { get; set; }
        public string Platform { get; set; }
        public string Domain { get; set; }
    }
}

