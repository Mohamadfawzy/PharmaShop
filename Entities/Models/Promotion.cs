using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string TitleEn { get; set; } = null!;

    public string? DescriptionEn { get; set; }

    public string PromoType { get; set; } = null!;

    public decimal? DiscountValue { get; set; }

    public int? ExtraPoints { get; set; }

    public int? FreeProductId { get; set; }

    public decimal? MinPurchaseAmount { get; set; }

    public int? MaxUsagePerCustomer { get; set; }

    public int? MaxUsageOverall { get; set; }

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<PromotionCategory> PromotionCategories { get; set; } = new List<PromotionCategory>();

    public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();

    public virtual ICollection<PromotionUsage> PromotionUsages { get; set; } = new List<PromotionUsage>();
}
