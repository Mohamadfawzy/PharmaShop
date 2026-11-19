namespace Shared.Models.Dtos.Delivery;

public sealed class AssignDeliveryAgentDto
{
    public int DeliveryAgentUserId { get; set; }
    public string? Notes { get; set; }
}

public sealed class DeliveryAgentQueryDto
{
    public int StoreId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class DeliveryAgentDto
{
    public int UserId { get; set; }
    public int EmployeeId { get; set; }
    public int PharmacyId { get; set; }
    public string FullNameAr { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}

public sealed class DeliveryAgentAssignmentDto
{
    public int OrderId { get; set; }
    public int DeliveryAgentUserId { get; set; }
    public int? EmployeeId { get; set; }
    public string FullNameAr { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTime AssignedAt { get; set; }
    public int? AssignedByUserId { get; set; }
    public string? Notes { get; set; }
}

public sealed class DeliveryOrdersQueryDto
{
    // 3=Preparing, 4=OutForDelivery. Other states are outside the agent workflow.
    public byte? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool NewestFirst { get; set; } = true;
}

public class DeliveryOrderListItemDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = null!;
    public int StoreId { get; set; }
    public string StoreNameAr { get; set; } = null!;
    public byte Status { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public string? DeliveryTitle { get; set; }
    public string DeliveryCity { get; set; } = null!;
    public string? DeliveryRegion { get; set; }
    public string DeliveryStreet { get; set; } = null!;
    public string? DeliveryPhone { get; set; }
    public DateTime AssignedAt { get; set; }
}

public sealed class DeliveryOrderDetailsDto : DeliveryOrderListItemDto
{
    public string? Notes { get; set; }
    public List<DeliveryOrderItemDto> Items { get; set; } = new();
}

public sealed class DeliveryOrderItemDto
{
    public int ProductId { get; set; }
    public string ProductNameAr { get; set; } = null!;
    public byte UnitLevel { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class UpdateDeliveryOrderStatusDto
{
    public byte Status { get; set; }
    public string? Notes { get; set; }
}

public sealed class OrderStatusHistoryDto
{
    public long Id { get; set; }
    public int OrderId { get; set; }
    public byte? PreviousStatus { get; set; }
    public byte NewStatus { get; set; }
    public int? ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public string ChangedByRole { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime ChangedAt { get; set; }
}
