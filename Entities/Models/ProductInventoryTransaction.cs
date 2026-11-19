namespace Entities.Models;

public class ProductInventoryTransaction
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public int? EmployeeId { get; set; }
    public int? OrderId { get; set; }
    public int? OrderItemId { get; set; }
    public int? ReturnRequestItemId { get; set; }
    public int? ProductUnitId { get; set; }
    public byte TransactionType { get; set; }
    public decimal Quantity { get; set; }
    public decimal QuantityBefore { get; set; }
    public decimal QuantityAfter { get; set; }
    public string Reason { get; set; } = null!;
    public string? BatchNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public decimal? CostPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual Product Product { get; set; } = null!;
    public virtual Employee? Employee { get; set; }
    public virtual Order? Order { get; set; }
    public virtual OrderItem? OrderItem { get; set; }
    public virtual ReturnRequestItem? ReturnRequestItem { get; set; }
}
