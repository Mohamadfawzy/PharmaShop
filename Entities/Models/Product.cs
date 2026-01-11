using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Product
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public int CategoryId { get; set; }

    public int? BrandId { get; set; }

    public string? Barcode { get; set; }

    public string? InternationalCode { get; set; }

    public string? StockProductCode { get; set; }

    public string Name { get; set; } = null!;

    public string NameEn { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Description { get; set; }

    public string? DescriptionEn { get; set; }

    public string? SearchKeywords { get; set; }

    public string? NormalizedName { get; set; }

    public string? NormalizedNameEn { get; set; }

    public string? DosageForm { get; set; }

    public string? Strength { get; set; }

    public string? PackSize { get; set; }

    public string? Unit { get; set; }

    public bool RequiresPrescription { get; set; }

    public bool EarnPoints { get; set; }

    public bool HasExpiry { get; set; }

    public bool AgeRestricted { get; set; }

    public int? MinAge { get; set; }

    public bool RequiresColdChain { get; set; }

    public bool ControlledSubstance { get; set; }

    public string? StorageConditions { get; set; }

    public bool IsTaxable { get; set; }

    public decimal VatRate { get; set; }

    public string? TaxCategoryCode { get; set; }

    public int MinOrderQty { get; set; }

    public int? MaxOrderQty { get; set; }

    public int? MaxPerDayQty { get; set; }

    public bool IsReturnable { get; set; }

    public int? ReturnWindowDays { get; set; }

    public bool AllowSplitSale { get; set; }

    public byte? SplitLevel { get; set; }

    public int? WeightGrams { get; set; }

    public int? LengthMm { get; set; }

    public int? WidthMm { get; set; }

    public int? HeightMm { get; set; }

    public bool TrackInventory { get; set; }

    public bool IsFeatured { get; set; }

    public bool IsActive { get; set; }

    public bool IsIntegrated { get; set; }

    public DateTime? IntegratedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? DeletedBy { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;

    public virtual ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();

    public virtual ProductImage? ProductImage { get; set; }

    public virtual ICollection<ProductInventory> ProductInventories { get; set; } = new List<ProductInventory>();

    public virtual ICollection<ProductTag> ProductTags { get; set; } = new List<ProductTag>();

    public virtual ProductUnit? ProductUnit { get; set; }

    public virtual ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
}
