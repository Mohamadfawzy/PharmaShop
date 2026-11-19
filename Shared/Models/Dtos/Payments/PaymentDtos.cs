namespace Shared.Models.Dtos.Payments;

public sealed class CreatePaymentAttemptDto
{
    public string Provider { get; set; } = string.Empty;
}

public sealed class UpdatePaymentAttemptDto
{
    public byte Status { get; set; }
    public string? ExternalReference { get; set; }
    public string? FailureReason { get; set; }
}

public sealed class CreatePaymentRefundDto
{
    public int PaymentAttemptId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class UpdatePaymentRefundDto
{
    public byte Status { get; set; }
    public string? ExternalReference { get; set; }
}

public sealed class PaymentAttemptDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public decimal Amount { get; set; }
    public byte Status { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PaymentRefundDto> Refunds { get; set; } = new();
}

public sealed class PaymentRefundDto
{
    public int Id { get; set; }
    public int PaymentAttemptId { get; set; }
    public decimal Amount { get; set; }
    public byte Status { get; set; }
    public string? ExternalReference { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? RequestedByUserId { get; set; }
    public int? ProcessedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
