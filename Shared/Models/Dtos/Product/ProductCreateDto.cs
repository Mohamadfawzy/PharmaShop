namespace Shared.Models.Dtos.Product;

using System.ComponentModel.DataAnnotations;

public  class ProductCreateDto
{
    [Required]
    public int StoreId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? CompanyId { get; set; }

    public decimal? ErpProductId { get; set; }
    public int? ErpStoreId { get; set; }

    [StringLength(50)]
    public string? InternationalCode { get; set; }

    [Required, StringLength(250)]
    public string NameAr { get; set; } = default!;

    [StringLength(250)]
    public string? NameEn { get; set; }

    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }

    [StringLength(500)]
    public string? SearchKeywords { get; set; }

    [Required]
    public int OuterUnitId { get; set; }

    public int? InnerUnitId { get; set; }
    public int? InnerPerOuter { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OuterUnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? InnerUnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int MinOrderQty { get; set; } = 1;

    public int? MaxOrderQty { get; set; }
    public int? MaxPerDayQty { get; set; }

    public bool IsReturnable { get; set; } = true;
    public bool IsRedeemableByPoints { get; set; }

    public bool AllowSplitSale { get; set; } = false;

    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; } = 0;

    public bool HasExpiry { get; set; } = true;
    public DateOnly? NearestExpiryDate { get; set; }

    public DateTime? LastStockSyncAt { get; set; }

    public bool HasPromotion { get; set; } = false;

    [Range(0, 100)]
    public decimal PromotionDiscountPercent { get; set; } = 0;

    public DateTime? PromotionStartsAt { get; set; }
    public DateTime? PromotionEndsAt { get; set; }

    public bool IsFeatured { get; set; } = false;

    [Range(0, int.MaxValue)]
    public int Points { get; set; } = 0;

    public bool RequiresPrescription { get; set; } = false;
    public bool IsAvailable { get; set; } = false;
    public bool IsActive { get; set; } = true;
}