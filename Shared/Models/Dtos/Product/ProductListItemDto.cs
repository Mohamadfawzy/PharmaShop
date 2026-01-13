using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class ProductListItemDto
{
    public int Id { get; init; }
    public int PharmacyId { get; init; }

    public int CategoryId { get; init; }
    public int? BrandId { get; init; }

    public string Name { get; init; } = default!;
    public string NameEn { get; init; } = default!;

    public string? Barcode { get; init; }
    public string? InternationalCode { get; init; }
    public string? StockProductCode { get; init; }

    public bool IsActive { get; init; }
    public bool RequiresPrescription { get; init; }
    public bool IsFeatured { get; init; }
    public bool TrackInventory { get; init; }
    public bool IsTaxable { get; init; }
    public decimal VatRate { get; init; }

    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }

    // optional (future)
    public string? PrimaryImageUrl { get; init; }

    // Stock summary (optional)
    public int? AvailableQty { get; init; }   // QuantityOnHand - ReservedQty (sum)
    public bool? IsInStock { get; init; }
    public bool? IsLowStock { get; init; }
}