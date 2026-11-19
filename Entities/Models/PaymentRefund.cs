namespace Entities.Models;

public class PaymentRefund
{
    public int Id { get; set; }
    public int PaymentAttemptId { get; set; }
    public decimal Amount { get; set; }
    public byte Status { get; set; } // 1=Requested, 2=Processing, 3=Succeeded, 4=Failed
    public string? ExternalReference { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? RequestedByUserId { get; set; }
    public int? ProcessedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public virtual PaymentAttempt PaymentAttempt { get; set; } = null!;
}
