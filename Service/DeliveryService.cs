using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Constants;
using Shared.Models.Dtos.Delivery;
using Shared.Models.Dtos.Order.order;
using Shared.Responses;

namespace Service;

public sealed class DeliveryService : IDeliveryService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IOrderService _orders;

    public DeliveryService(IUnitOfWork uow, ICurrentUserService currentUser, IOrderService orders)
    { _uow = uow; _currentUser = currentUser; _orders = orders; }

    public async Task<AppResponse<List<DeliveryAgentDto>>> GetAgentsAsync(DeliveryAgentQueryDto query, CancellationToken ct)
    {
        if (query is null || query.StoreId <= 0) return AppResponse<List<DeliveryAgentDto>>.ValidationError("StoreId is required");
        query.Page = Math.Max(1, query.Page); query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        var pharmacyId = await _uow.Orders.GetStorePharmacyIdAsync(query.StoreId, ct);
        if (!pharmacyId.HasValue) return AppResponse<List<DeliveryAgentDto>>.NotFound("Store not found");
        if (!_currentUser.IsInRole(AppRoles.Admin))
        {
            if (!int.TryParse(_currentUser.UserId, out var managerUserId))
                return AppResponse<List<DeliveryAgentDto>>.Unauthorized("User identity is invalid");
            var manager = await _uow.Employees.FirstOrDefaultAsync(e => e.UserId == managerUserId && e.IsActive && e.DeletedAt == null, ct: ct);
            if (manager is null || manager.PharmacyId != pharmacyId.Value)
                return AppResponse<List<DeliveryAgentDto>>.Forbidden("You cannot view delivery agents for this pharmacy");
        }
        var agents = await _uow.Orders.SearchDeliveryAgentsAsync(query.StoreId, query.Page, query.PageSize, ct);
        return AppResponse<List<DeliveryAgentDto>>.Ok(agents);
    }

    public async Task<AppResponse<DeliveryAgentAssignmentDto>> AssignAsync(int orderId, AssignDeliveryAgentDto dto, CancellationToken ct)
    {
        if (orderId <= 0 || dto is null || dto.DeliveryAgentUserId <= 0 || dto.Notes?.Trim().Length > 500)
            return AppResponse<DeliveryAgentAssignmentDto>.ValidationError("OrderId and DeliveryAgentUserId are required; Notes must be at most 500 characters");
        if (!int.TryParse(_currentUser.UserId, out var assigningUserId))
            return AppResponse<DeliveryAgentAssignmentDto>.Unauthorized("User identity is invalid");
        await using var transaction = await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.Orders.LockOrderForStatusUpdateAsync(orderId, ct);
            var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
            if (order is null) return AppResponse<DeliveryAgentAssignmentDto>.NotFound("Order not found");
            if (!await CanManageOrderStoreAsync(order, ct))
                return AppResponse<DeliveryAgentAssignmentDto>.Forbidden("You cannot manage delivery assignments for another pharmacy");
            if (order.Status is not (3 or 4))
                return AppResponse<DeliveryAgentAssignmentDto>.BusinessRuleViolation("A delivery agent can only be assigned to a preparing or out-for-delivery order");
            var pharmacyId = await _uow.Orders.GetStorePharmacyIdAsync(order.StoreId, ct);
            var agent = await _uow.Orders.GetActiveDeliveryAgentAsync(dto.DeliveryAgentUserId, ct);
            if (agent is null || agent.PharmacyId != pharmacyId)
                return AppResponse<DeliveryAgentAssignmentDto>.ValidationError("An active delivery agent from the order's pharmacy is required");
            if (!_currentUser.IsInRole(AppRoles.Admin))
            {
                var manager = await _uow.Employees.FirstOrDefaultAsync(e => e.UserId == assigningUserId && e.IsActive && e.DeletedAt == null, ct: ct);
                if (manager is null || manager.PharmacyId != pharmacyId)
                    return AppResponse<DeliveryAgentAssignmentDto>.Forbidden("You cannot assign delivery agents for this pharmacy");
            }

            var now = DateTime.UtcNow;
            var old = await _uow.Orders.GetActiveDeliveryAssignmentAsync(orderId, true, ct);
            if (old?.DeliveryAgentUserId == dto.DeliveryAgentUserId)
            {
                await transaction.CommitAsync(ct);
                return await _uow.Orders.GetDeliveryAssignmentDetailsAsync(orderId, ct) is { } current
                    ? AppResponse<DeliveryAgentAssignmentDto>.Ok(current, "Delivery agent is already assigned")
                    : AppResponse<DeliveryAgentAssignmentDto>.NotFound("Assignment not found");
            }
            if (old is not null)
            {
                old.IsActive = false; old.UnassignedAt = now;
                _uow.Orders.UpdateDeliveryAssignment(old);
                await _uow.CompleteAsync(ct);
            }
            var assignment = new OrderDeliveryAssignment
            {
                OrderId = orderId, DeliveryAgentUserId = dto.DeliveryAgentUserId,
                AssignedByUserId = assigningUserId, AssignedAt = now, IsActive = true,
                Notes = dto.Notes?.Trim()
            };
            await _uow.Orders.AddDeliveryAssignmentAsync(assignment, ct);
            await _uow.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return AppResponse<DeliveryAgentAssignmentDto>.Ok(new DeliveryAgentAssignmentDto
            {
                OrderId = orderId, DeliveryAgentUserId = agent.UserId!.Value, EmployeeId = agent.Id,
                FullNameAr = agent.FullNameAr, PhoneNumber = agent.PhoneNumber,
                AssignedAt = now, AssignedByUserId = assigningUserId, Notes = assignment.Notes
            }, "Delivery agent assigned successfully");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AppResponse<int>> UnassignAsync(int orderId, CancellationToken ct)
    {
        if (orderId <= 0) return AppResponse<int>.ValidationError("Invalid order id");
        await using var transaction = await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.Orders.LockOrderForStatusUpdateAsync(orderId, ct);
            var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
            if (order is null) return AppResponse<int>.NotFound("Order not found");
            if (!await CanManageOrderStoreAsync(order, ct)) return AppResponse<int>.Forbidden("You cannot manage orders for another pharmacy");
            if (order.Status is not (3 or 4)) return AppResponse<int>.BusinessRuleViolation("This order can no longer be unassigned");
            var assignment = await _uow.Orders.GetActiveDeliveryAssignmentAsync(orderId, true, ct);
            if (assignment is null) return AppResponse<int>.NotFound("Active delivery assignment not found");
            assignment.IsActive = false; assignment.UnassignedAt = DateTime.UtcNow;
            assignment.UnassignedByUserId = int.TryParse(_currentUser.UserId, out var unassignedByUserId) ? unassignedByUserId : null;
            _uow.Orders.UpdateDeliveryAssignment(assignment);
            await _uow.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return AppResponse<int>.Ok(orderId, "Delivery agent unassigned");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AppResponse<DeliveryAgentAssignmentDto>> GetAssignmentAsync(int orderId, CancellationToken ct)
    {
        if (orderId <= 0) return AppResponse<DeliveryAgentAssignmentDto>.ValidationError("Invalid order id");
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
        if (order is null) return AppResponse<DeliveryAgentAssignmentDto>.NotFound("Order not found");
        if (!await CanManageOrderStoreAsync(order, ct))
            return AppResponse<DeliveryAgentAssignmentDto>.Forbidden("You cannot view delivery assignments for another pharmacy");
        var assignment = await _uow.Orders.GetDeliveryAssignmentDetailsAsync(orderId, ct);
        return assignment is null ? AppResponse<DeliveryAgentAssignmentDto>.NotFound("No active delivery agent assignment")
            : AppResponse<DeliveryAgentAssignmentDto>.Ok(assignment);
    }

    public async Task<AppResponse<List<DeliveryOrderListItemDto>>> GetMyOrdersAsync(DeliveryOrdersQueryDto query, CancellationToken ct)
    {
        if (query is null) return AppResponse<List<DeliveryOrderListItemDto>>.ValidationError("Query is required");
        if (query.Status.HasValue && query.Status is not (3 or 4))
            return AppResponse<List<DeliveryOrderListItemDto>>.ValidationError("Delivery agents can list only statuses 3 (Preparing) and 4 (OutForDelivery)");
        if (!int.TryParse(_currentUser.UserId, out var userId) || await _uow.Orders.GetActiveDeliveryAgentAsync(userId, ct) is null)
            return AppResponse<List<DeliveryOrderListItemDto>>.Unauthorized("An active delivery-agent account is required");
        query.Page = Math.Max(1, query.Page); query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        var result = await _uow.Orders.SearchAssignedDeliveryOrdersAsync(userId, query, ct);
        return AppResponse<List<DeliveryOrderListItemDto>>.Ok(result.Items,
            PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount));
    }

    public async Task<AppResponse<DeliveryOrderDetailsDto>> GetMyOrderAsync(int orderId, CancellationToken ct)
    {
        if (orderId <= 0) return AppResponse<DeliveryOrderDetailsDto>.ValidationError("Invalid order id");
        if (!int.TryParse(_currentUser.UserId, out var userId) || await _uow.Orders.GetActiveDeliveryAgentAsync(userId, ct) is null)
            return AppResponse<DeliveryOrderDetailsDto>.Unauthorized("An active delivery-agent account is required");
        var order = await _uow.Orders.GetAssignedDeliveryOrderAsync(orderId, userId, ct);
        return order is null ? AppResponse<DeliveryOrderDetailsDto>.NotFound("Assigned delivery order not found")
            : AppResponse<DeliveryOrderDetailsDto>.Ok(order);
    }

    public async Task<AppResponse<int>> UpdateMyOrderStatusAsync(int orderId, UpdateDeliveryOrderStatusDto dto, CancellationToken ct)
    {
        if (orderId <= 0 || dto is null || dto.Status != 5 || dto.Notes?.Trim().Length > 500)
            return AppResponse<int>.ValidationError("Delivery agents may only set Status to 5 (Delivered); Notes must be at most 500 characters");
        if (!int.TryParse(_currentUser.UserId, out var userId) || await _uow.Orders.GetActiveDeliveryAgentAsync(userId, ct) is null)
            return AppResponse<int>.Unauthorized("An active delivery-agent account is required");
        var order = await _uow.Orders.GetAssignedDeliveryOrderAsync(orderId, userId, ct);
        if (order is null || order.Status != 4)
            return AppResponse<int>.BusinessRuleViolation("Only your assigned out-for-delivery orders can be marked delivered");
        return await _orders.UpdateOrderStatusAsync(orderId, new OrderStatusUpdateDto { Status = 5, Notes = dto.Notes }, ct);
    }

    public async Task<AppResponse<List<OrderStatusHistoryDto>>> GetOrderStatusHistoryAsync(int orderId, CancellationToken ct)
    {
        if (orderId <= 0) return AppResponse<List<OrderStatusHistoryDto>>.ValidationError("Invalid order id");
        var order = await _uow.Orders.GetOrderForStatusUpdateAsync(orderId, ct);
        if (order is null) return AppResponse<List<OrderStatusHistoryDto>>.NotFound("Order not found");
        if (!await CanManageOrderStoreAsync(order, ct)) return AppResponse<List<OrderStatusHistoryDto>>.Forbidden("You cannot view order history for another pharmacy");
        return AppResponse<List<OrderStatusHistoryDto>>.Ok(await _uow.Orders.GetOrderStatusHistoryAsync(orderId, ct));
    }

    private async Task<bool> CanManageOrderStoreAsync(Order order, CancellationToken ct)
    {
        if (_currentUser.IsInRole(AppRoles.Admin)) return true;
        if (!int.TryParse(_currentUser.UserId, out var userId)) return false;
        var manager = await _uow.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive && e.DeletedAt == null, ct: ct);
        if (manager is null) return false;
        var pharmacyId = await _uow.Orders.GetStorePharmacyIdAsync(order.StoreId, ct);
        return pharmacyId == manager.PharmacyId;
    }
}
