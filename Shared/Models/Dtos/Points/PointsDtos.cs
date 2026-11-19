namespace Shared.Models.Dtos.Points;

public sealed class PointsBalanceDto
{
    public int AvailablePoints { get; set; }
    public int PendingPoints { get; set; }
    public int ExpiringWithin30Days { get; set; }
    public DateTime? NextExpiryAt { get; set; }
    public decimal AvailableValueEGP { get; set; }
}

public sealed class PointsListQueryDto
{
    public byte? Status { get; set; }
    public byte? TransactionType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CustomerPointLotDto
{
    public int Id { get; set; }
    public int? OrderId { get; set; }
    public int PointsTotal { get; set; }
    public int RemainingPoints { get; set; }
    public byte Status { get; set; }
    public DateTime EarnedAt { get; set; }
    public DateTime? AvailableAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public sealed class CustomerPointTransactionDto
{
    public long Id { get; set; }
    public byte TransactionType { get; set; }
    public int PointsDelta { get; set; }
    public int? BalanceAfter { get; set; }
    public int? OrderId { get; set; }
    public int? OrderItemId { get; set; }
    public int? LotId { get; set; }
    public byte? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class RedeemablePointsProductDto
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public string NameAr { get; set; } = default!;
    public string? NameEn { get; set; }
    public decimal OuterUnitPrice { get; set; }
    public decimal? InnerUnitPrice { get; set; }
    public decimal AvailableQuantity { get; set; }
    public int OuterUnitId { get; set; }
    public int? InnerUnitId { get; set; }
    public int? InnerPerOuter { get; set; }
    public bool AllowSplitSale { get; set; }
}

public sealed class ManualPointsAdjustmentDto
{
    public int PointsDelta { get; set; }
    public int ReferenceId { get; set; }
    public string Reason { get; set; } = default!;
}

public sealed class ManualPointsAdjustmentResultDto
{
    public int CustomerId { get; set; }
    public int PointsDelta { get; set; }
    public int BalanceAfter { get; set; }
    public long TransactionId { get; set; }
    public int? LotId { get; set; }
}
