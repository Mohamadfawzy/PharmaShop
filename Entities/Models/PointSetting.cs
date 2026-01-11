using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PointSetting
{
    public int PharmacyId { get; set; }

    public bool EarnEnabled { get; set; }

    public decimal EarnPerAmount { get; set; }

    public int EarnPoints { get; set; }

    public bool RedeemEnabled { get; set; }

    public decimal PointValueEGP { get; set; }

    public decimal MaxRedeemPercent { get; set; }

    public int? PointsExpireDays { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
