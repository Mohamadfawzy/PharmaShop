using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public decimal? ErpPgoId { get; set; }

    public string? Name { get; set; }

    public string? Notes { get; set; }

    public DateTime? StartAt { get; set; }

    public DateTime? EndAt { get; set; }

    public int? TotalAmount { get; set; }

    public int? BasicAmount { get; set; }

    public int? OfferAmount { get; set; }

    public decimal? DiscountPercent { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
}
