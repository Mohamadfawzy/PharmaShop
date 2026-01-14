using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class OpenBoxResultDto
{
    public int ProductId { get; init; }
    public int StoreId { get; init; }

    public int FromProductUnitId { get; init; }
    public int ToProductUnitId { get; init; }

    public int FromQtyConverted { get; init; }
    public int ToQtyCreated { get; init; }

    public long UsedFromBatchId { get; init; }
    public string BatchNumber { get; init; } = default!;
    public DateTime? ExpirationDate { get; init; }

    public int FromQtyOnHandAfter { get; init; }
    public int ToQtyOnHandAfter { get; init; }
}