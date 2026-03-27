using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;

public sealed class ReceiveStockResultDto
{
    public int ProductId { get; init; }
    public int StoreId { get; init; }
    public int ProductUnitId { get; init; }

    public long BatchId { get; init; }
    public string BatchNumber { get; init; } = default!;
    public DateTime? ExpirationDate { get; init; }

    public int QuantityReceived { get; init; }

    public int BatchQtyOnHandAfter { get; init; }
    public int InventoryQtyOnHandAfter { get; init; }
}