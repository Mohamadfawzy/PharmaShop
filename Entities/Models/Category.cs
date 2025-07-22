using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ParentCategoryId { get; set; }

    public virtual ICollection<PromoCode> PromoCodes { get; set; } = new List<PromoCode>();

    public virtual ICollection<PromotionCategory> PromotionCategories { get; set; } = new List<PromotionCategory>();

    public virtual ICollection<Sub1Category> Sub1Categories { get; set; } = new List<Sub1Category>();

    public virtual ICollection<Sub2Category> Sub2Categories { get; set; } = new List<Sub2Category>();
}
