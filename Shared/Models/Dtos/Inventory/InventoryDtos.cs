namespace Shared.Models.Dtos.Inventory;

public sealed class AdjustInventoryDto
{
    public int ProductId { get; set; }
    // Positive adds stock; negative removes stock. Quantity is in the product's outer unit.
    public decimal QuantityDelta { get; set; }
    public string Reason { get; set; } = null!;
}

public sealed class InventoryTransactionQueryDto
{
    public int? ProductId { get; set; }
    public int? StoreId { get; set; }
    public int? EmployeeId { get; set; }
    public int? OrderId { get; set; }
    public byte? TransactionType { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class InventoryTransactionDto
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int StoreId { get; set; }
    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? OrderId { get; set; }
    public int? OrderItemId { get; set; }
    public byte TransactionType { get; set; }
    public int? ProductUnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public string Reason { get; set; } = null!;
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal? CostPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class InventoryTransactionPageDto
{
    public List<InventoryTransactionDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
