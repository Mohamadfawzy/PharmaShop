namespace Entities.Models;

public class OrderStatusHistory
{
    public long Id { get; set; }
    public int OrderId { get; set; }
    public byte? PreviousStatus { get; set; }
    public byte NewStatus { get; set; }
    public int? ChangedByUserId { get; set; }
    public string ChangedByRole { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
    public virtual Order Order { get; set; } = null!;
    public virtual AspNetUser? ChangedByUser { get; set; }
}
