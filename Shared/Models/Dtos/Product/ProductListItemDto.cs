using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;

public sealed class ProductListItemDto
{
    public int Id { get; set; }

    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }

    public decimal OuterUnitPrice { get; set; }

    public bool HasPromotion { get; set; }
    public decimal PromotionDiscountPercent { get; set; }

    public int Points { get; set; }

    public bool RequiresPrescription { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsActive { get; set; }

    // Optional: how many tags attached to product
    public int TagsCount { get; set; }
}