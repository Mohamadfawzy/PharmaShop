namespace Shared.Models.Dtos.Order;

public sealed class CustomerOrderListQueryDto
{
    public byte? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool NewestFirst { get; set; } = true;
}

public class CustomerOrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = default!;
    public int StoreId { get; set; }
    public byte OrderSource { get; set; }
    public byte Status { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public byte PaymentStatus { get; set; }
    public decimal GrandTotal { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerOrderDetailsDto : CustomerOrderListItemDto
{
    public byte PaymentMethod { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ItemsDiscountTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public int RedeemedPoints { get; set; }
    public decimal RedeemedAmount { get; set; }
    public int EarnedPoints { get; set; }
    public int? PrescriptionId { get; set; }
    public string? DeliveryTitle { get; set; }
    public string DeliveryCity { get; set; } = default!;
    public string? DeliveryRegion { get; set; }
    public string DeliveryStreet { get; set; } = default!;
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public string? DeliveryPhone { get; set; }
    public string? Notes { get; set; }
    public List<OrderItemSnapshotDto> Items { get; set; } = [];
}

public sealed class AdminOrderDetailsDto : CustomerOrderDetailsDto
{
    public int CustomerId { get; set; }
    public string CustomerNameAr { get; set; } = default!;
    public string? CustomerPhone { get; set; }
    public string? InternalNotes { get; set; }
}

public sealed class OrderItemSnapshotDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNameAr { get; set; } = default!;
    public string? ProductNameEn { get; set; }
    public byte UnitLevel { get; set; }
    public decimal Quantity { get; set; }
    public int OuterUnitIdSnapshot { get; set; }
    public int? InnerUnitIdSnapshot { get; set; }
    public int? InnerPerOuterSnapshot { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public decimal DiscountPercentSnapshot { get; set; }
    public decimal FinalUnitPriceSnapshot { get; set; }
    public decimal LineTotal { get; set; }
    public int PointsSnapshot { get; set; }
    public int EarnedPoints { get; set; }
    public int? AppliedPromotionId { get; set; }
}
