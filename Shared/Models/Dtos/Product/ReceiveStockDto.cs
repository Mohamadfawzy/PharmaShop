using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;

public sealed class ReceiveStockDto
{
    public int StoreId { get; init; }

    // الوحدة التي تم الاستلام عليها (مثال: Carton أو Box)
    public int ProductUnitId { get; init; }

    public int Quantity { get; init; }

    // Lot/Batch info
    public string BatchNumber { get; init; } = default!;
    public DateTime? ExpirationDate { get; init; } // required if product.HasExpiry = true

    // Optional
    public decimal? CostPrice { get; init; } // optional (if you store per batch cost)
    public string? Reason { get; init; }     // for future audit/ledger
}