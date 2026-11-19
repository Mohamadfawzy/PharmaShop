namespace Entities.Models;

public class ReturnRequestItem
{
    public int Id { get; set; }
    public int ReturnRequestId { get; set; }
    public int OrderItemId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ReturnRequest ReturnRequest { get; set; } = null!;
    public virtual OrderItem OrderItem { get; set; } = null!;
    public virtual ICollection<ProductInventoryTransaction> InventoryTransactions { get; set; } = new List<ProductInventoryTransaction>();
}
