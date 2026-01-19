using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class StockAdjustmentResultDto
{
    public int ProductId { get; init; }
    public int StoreId { get; init; }
    public int ProductUnitId { get; init; }

    public long BatchId { get; init; }
    public string BatchNumber { get; init; } = default!;
    public DateTime? ExpirationDate { get; init; }

    public int OldBatchQty { get; init; }
    public int NewBatchQty { get; init; }
    public int DeltaQty { get; init; } // New - Old

    public int InventoryQtyOnHandAfter { get; init; }
}
