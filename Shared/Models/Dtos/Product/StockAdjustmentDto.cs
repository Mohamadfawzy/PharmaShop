using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Dtos.Product;


public sealed class StockAdjustmentDto
{
    public int StoreId { get; init; }
    public int ProductUnitId { get; init; }

    // تحديد الباتش:
    // إما BatchId أو (BatchNumber + ExpirationDate)
    public long? BatchId { get; init; }
    public string? BatchNumber { get; init; }
    public DateTime? ExpirationDate { get; init; }

    // الكمية الفعلية بعد الجرد (Absolute)
    public int CountedQty { get; init; }

    // Optional
    public decimal? CostPrice { get; init; } // لو حابب تحديث تكلفة الدفعة أثناء الجرد
    public string? Reason { get; init; }     // TODO: audit/ledger later
}