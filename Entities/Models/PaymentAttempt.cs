namespace Entities.Models;

public class PaymentAttempt
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public decimal Amount { get; set; }
    public byte Status { get; set; } // 1=Pending, 2=Succeeded, 3=Failed, 4=Cancelled
    public string? FailureReason { get; set; }
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public virtual Order Order { get; set; } = null!;
}
