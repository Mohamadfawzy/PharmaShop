using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product.Units;


public sealed class ProductUnitCreateDto
{
    public int UnitId { get; init; }                      // FK -> Units
    public int? ParentProductUnitId { get; init; }        // optional hierarchy
    public decimal? UnitsPerParent { get; init; }         // optional, >0 when Parent exists

    public int? BaseUnitId { get; init; }                 // optional content unit (Tablet/ml)
    public decimal? BaseQuantity { get; init; }           // optional, >0 when BaseUnit exists

    public string CurrencyCode { get; init; } = "EGP";    // default
    public decimal? CostPrice { get; init; }              // optional
    public decimal ListPrice { get; init; }               // required

    public string? UnitCode { get; init; }                // optional unique within pharmacy
    public string? SKU { get; init; }                     // optional unique within pharmacy

    public int SortOrder { get; init; } = 0;
    public bool IsPrimary { get; init; } = false;
    public bool IsActive { get; init; } = true;

    // optional audit reason
    public string? Reason { get; init; }
}