using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Product
{
    public int Id { get; set; }

    public int StoreId { get; set; }

    public int CategoryId { get; set; }

    public int? CompanyId { get; set; }

    public decimal? ErpProductId { get; set; }

    public string? InternationalCode { get; set; }

    public string NameAr { get; set; } = null!;

    public string? NameEn { get; set; }

    public string? DescriptionAr { get; set; }

    public string? DescriptionEn { get; set; }

    public string? SearchKeywords { get; set; }

    public int OuterUnitId { get; set; }

    public int? InnerUnitId { get; set; }

    public int? InnerPerOuter { get; set; }

    public decimal OuterUnitPrice { get; set; }

    public decimal? InnerUnitPrice { get; set; }

    public int MinOrderQty { get; set; }

    public int? MaxOrderQty { get; set; }

    public int? MaxPerDayQty { get; set; }

    public bool IsReturnable { get; set; }

    public bool AllowSplitSale { get; set; }

    public decimal Quantity { get; set; }

    public bool HasExpiry { get; set; }

    public DateOnly? NearestExpiryDate { get; set; }

    public DateTime? LastStockSyncAt { get; set; }

    public bool HasPromotion { get; set; }

    public decimal PromotionDiscountPercent { get; set; }

    public DateTime? PromotionStartsAt { get; set; }

    public DateTime? PromotionEndsAt { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsIntegrated { get; set; }

    public DateTime? IntegratedAt { get; set; }

    public int Points { get; set; }

    public bool RequiresPrescription { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Company? Company { get; set; }

    public virtual Unit? InnerUnit { get; set; }

    public virtual Unit OuterUnit { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

    public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();

    public virtual Store Store { get; set; } = null!;
}
