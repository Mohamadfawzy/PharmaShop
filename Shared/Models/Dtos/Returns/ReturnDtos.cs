namespace Shared.Models.Dtos.Returns;

public sealed class CancelOrderDto
{
    public string Reason { get; set; } = null!;
}

public sealed class CreateReturnRequestDto
{
    public string Reason { get; set; } = null!;
    public List<CreateReturnRequestItemDto> Items { get; set; } = new();
}

public sealed class CreateReturnRequestItemDto
{
    public int OrderItemId { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class ReviewReturnRequestDto
{
    public bool Approve { get; set; }
    public string? Notes { get; set; }
}

public sealed class ReturnRequestQueryDto
{
    public byte? Status { get; set; }
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class ReturnRequestItemDto
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductNameAr { get; set; } = null!;
    public byte UnitLevel { get; set; }
    public decimal RequestedQuantity { get; set; }
    public decimal PurchasedQuantity { get; set; }
    public decimal FinalUnitPrice { get; set; }
    public decimal LineRefundValue { get; set; }
}

public sealed class ReturnRequestDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int CustomerId { get; set; }
    public string CustomerNameAr { get; set; } = null!;
    public byte Status { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public int? ReviewedByEmployeeId { get; set; }
    public int? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public List<ReturnRequestItemDto> Items { get; set; } = new();
}

public sealed class ReturnRequestListItemDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int CustomerId { get; set; }
    public string CustomerNameAr { get; set; } = null!;
    public byte Status { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public int ItemsCount { get; set; }
}

public sealed class ReturnReviewResultDto
{
    public int ReturnRequestId { get; set; }
    public byte Status { get; set; }
    public decimal RestockedOuterQuantity { get; set; }
    public int RedeemedPointsRefunded { get; set; }
    public int EarnedPointsReversed { get; set; }
}
