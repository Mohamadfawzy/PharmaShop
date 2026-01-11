using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class ProductDetailsDto
{
    public int Id { get; init; }

    // 🔐 Concurrency
    public byte[] RowVersion { get; init; } = default!;

    public int CategoryId { get; init; }
    public int? BrandId { get; init; }

    public string? Barcode { get; init; }
    public string? InternationalCode { get; init; }
    public string? StockProductCode { get; init; }

    public string Name { get; init; } = default!;
    public string NameEn { get; init; } = default!;

    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? DescriptionEn { get; init; }

    public string? SearchKeywords { get; init; }

    public string? DosageForm { get; init; }
    public string? Strength { get; init; }
    public string? PackSize { get; init; }
    public string? Unit { get; init; }

    public bool RequiresPrescription { get; init; }
    public bool EarnPoints { get; init; }
    public bool HasExpiry { get; init; }

    public bool AgeRestricted { get; init; }
    public int? MinAge { get; init; }

    public bool RequiresColdChain { get; init; }
    public bool ControlledSubstance { get; init; }
    public string? StorageConditions { get; init; }

    public bool IsTaxable { get; init; }
    public decimal VatRate { get; init; }
    public string? TaxCategoryCode { get; init; }

    public int MinOrderQty { get; init; }
    public int? MaxOrderQty { get; init; }
    public int? MaxPerDayQty { get; init; }

    public bool IsReturnable { get; init; }
    public int? ReturnWindowDays { get; init; }

    public bool AllowSplitSale { get; init; }
    public byte? SplitLevel { get; init; }

    public int? WeightGrams { get; init; }
    public int? LengthMm { get; init; }
    public int? WidthMm { get; init; }
    public int? HeightMm { get; init; }

    public bool TrackInventory { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
