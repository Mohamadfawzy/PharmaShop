namespace Shared.Models.Dtos.Product;

public class ProductUpdateDto
{
    // ✅ Concurrency (required)
    // JSON سيرسلها Base64 تلقائياً عند استخدام byte[]
    public byte[] RowVersion { get; init; } = default!;

    public int CategoryId { get; init; }
    public int? BrandId { get; init; }

    // Unified codes (optional but unique per pharmacy when provided)
    public string? Barcode { get; init; }
    public string? InternationalCode { get; init; }
    public string? StockProductCode { get; init; }

    // Names
    public string Name { get; init; } = default!;
    public string NameEn { get; init; } = default!;

    // Optional text
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? DescriptionEn { get; init; }
    public string? AuditReason { get; init; }

    // Search helpers
    public string? SearchKeywords { get; init; }

    // Packaging info (informational)
    public string? DosageForm { get; init; }
    public string? Strength { get; init; }
    public string? PackSize { get; init; }
    public string? Unit { get; init; }

    // Compliance / rules
    public bool RequiresPrescription { get; init; }
    public bool EarnPoints { get; init; }
    public bool HasExpiry { get; init; }

    public bool AgeRestricted { get; init; }
    public int? MinAge { get; init; }

    public bool RequiresColdChain { get; init; }
    public bool ControlledSubstance { get; init; }
    public string? StorageConditions { get; init; }

    // Taxes
    public bool IsTaxable { get; init; }
    public decimal VatRate { get; init; }
    public string? TaxCategoryCode { get; init; }

    // Order limits
    public int MinOrderQty { get; init; }
    public int? MaxOrderQty { get; init; }
    public int? MaxPerDayQty { get; init; }

    public bool IsReturnable { get; init; }
    public int? ReturnWindowDays { get; init; }

    // Split sale policy
    public bool AllowSplitSale { get; init; }
    public byte? SplitLevel { get; init; } // 1=Inner(Strip) | 2=Base(Tablet)

    // Shipping / dimensions
    public int? WeightGrams { get; init; }
    public int? LengthMm { get; init; }
    public int? WidthMm { get; init; }
    public int? HeightMm { get; init; }

    // Flags
    public bool TrackInventory { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsActive { get; init; }
}
