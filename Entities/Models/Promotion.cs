using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public byte PromotionType { get; set; }

    public decimal? DiscountValue { get; set; }

    public DateTime StartAt { get; set; }

    public DateTime EndAt { get; set; }

    public int Priority { get; set; }

    public bool IsStackable { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
