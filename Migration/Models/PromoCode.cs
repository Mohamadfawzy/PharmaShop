using System;
using System.Collections.Generic;

namespace Migration.Models;

public partial class PromoCode
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    public string PromoType { get; set; } = null!;

    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    public int? ExtraPointsValue { get; set; }

    public int? MaxUsageCount { get; set; }

    public int UsedCount { get; set; }

    public bool IsSingleUsePerCustomer { get; set; }

    public int? CustomerId { get; set; }

    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Product? Product { get; set; }

    public virtual ICollection<PromoCodeUsage> PromoCodeUsages { get; set; } = new List<PromoCodeUsage>();
}
