using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Models.Dtos.Order.order;
using Shared.Models.Dtos.Returns;
using Shared.Responses;

namespace Service;

public sealed class ReturnRequestService : IReturnRequestService
{
    private const byte Pending = 1, Approved = 2, Rejected = 3;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IOrderService _orders;
    private readonly IInventoryService _inventory;
    private readonly IPointsService _points;

    public ReturnRequestService(IUnitOfWork uow, ICurrentUserService currentUser, IOrderService orders,
        IInventoryService inventory, IPointsService points)
    { _uow = uow; _currentUser = currentUser; _orders = orders; _inventory = inventory; _points = points; }

    public async Task<AppResponse<int>> CancelMyOrderAsync(int orderId, CancelOrderDto dto, CancellationToken ct)
    {
        if (orderId <= 0 || dto is null || string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length > 450)
            return AppResponse<int>.ValidationError("A valid order id and cancellation reason are required");
        var customer = await FindCurrentCustomerAsync(ct);
        if (customer is null) return AppResponse<int>.Unauthorized("An active customer account is required");
        var order = await _uow.ReturnRequests.GetCustomerOrderAsync(orderId, customer.Id, false, ct);
        if (order is null) return AppResponse<int>.NotFound("Order not found");
        if (order.Status is not (1 or 2 or 3))
            return AppResponse<int>.BusinessRuleViolation("This order can no longer be cancelled");
        return await _orders.UpdateOrderStatusAsync(orderId, new OrderStatusUpdateDto
        {
            Status = 6, Notes = $"Customer cancellation: {dto.Reason.Trim()}"
        }, ct);
    }

    public async Task<AppResponse<ReturnRequestDto>> CreateMyReturnAsync(int orderId, CreateReturnRequestDto dto, CancellationToken ct)
    {
        if (orderId <= 0 || dto is null) return AppResponse<ReturnRequestDto>.ValidationError("Order id and request body are required");
        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length > 500)
            return AppResponse<ReturnRequestDto>.ValidationError("Reason is required and cannot exceed 500 characters");
        if (dto.Items is null || dto.Items.Count == 0 || dto.Items.Count > 50)
            return AppResponse<ReturnRequestDto>.ValidationError("Return request must contain between 1 and 50 items");
        if (dto.Items.Any(i => i.OrderItemId <= 0 || i.Quantity <= 0 || decimal.Round(i.Quantity, 3) != i.Quantity)
            || dto.Items.Select(i => i.OrderItemId).Distinct().Count() != dto.Items.Count)
            return AppResponse<ReturnRequestDto>.ValidationError("Each item must have a unique positive OrderItemId and Quantity");
        var customer = await FindCurrentCustomerAsync(ct);
        if (customer is null) return AppResponse<ReturnRequestDto>.Unauthorized("An active customer account is required");

        await using var transaction = await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.ReturnRequests.LockOrderForReturnsAsync(orderId, ct);
            var order = await _uow.ReturnRequests.GetCustomerOrderAsync(orderId, customer.Id, true, ct);
            if (order is null) return AppResponse<ReturnRequestDto>.NotFound("Order not found");
            if (order.Status != 5) return AppResponse<ReturnRequestDto>.BusinessRuleViolation("Only delivered orders can be returned");
            var byId = order.OrderItems.ToDictionary(i => i.Id);
            var requestedIds = dto.Items.Select(i => i.OrderItemId).ToArray();
            var reserved = await _uow.ReturnRequests.GetReservedQuantitiesAsync(requestedIds, ct);
            foreach (var requestItem in dto.Items)
            {
                if (!byId.TryGetValue(requestItem.OrderItemId, out var orderItem))
                    return AppResponse<ReturnRequestDto>.ValidationError($"Order item {requestItem.OrderItemId} does not belong to this order");
                if (!orderItem.Product.IsReturnable)
                    return AppResponse<ReturnRequestDto>.BusinessRuleViolation($"Product {orderItem.Product.NameAr} is not returnable");
                var alreadyRequested = reserved.GetValueOrDefault(orderItem.Id);
                if (requestItem.Quantity > orderItem.Quantity - alreadyRequested)
                    return AppResponse<ReturnRequestDto>.BusinessRuleViolation($"Return quantity exceeds the remaining quantity for order item {orderItem.Id}");
            }

            var now = DateTime.UtcNow;
            var request = new ReturnRequest
            {
                OrderId = order.Id, CustomerId = customer.Id, Status = Pending,
                Reason = dto.Reason.Trim(), RequestedAt = now, CreatedAt = now,
                Items = dto.Items.Select(i => new ReturnRequestItem
                {
                    OrderItemId = i.OrderItemId, OrderItem = byId[i.OrderItemId], Quantity = i.Quantity, CreatedAt = now
                }).ToList()
            };
            await _uow.ReturnRequests.AddAsync(request, ct);
            await _uow.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return AppResponse<ReturnRequestDto>.Created(Map(request, order, customer), $"/api/v1/orders/{order.Id}/returns/{request.Id}", "Return request submitted");
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AppResponse<ReturnRequestDto>> GetMyReturnAsync(int orderId, int returnRequestId, CancellationToken ct)
    {
        var customer = await FindCurrentCustomerAsync(ct);
        if (customer is null) return AppResponse<ReturnRequestDto>.Unauthorized("An active customer account is required");
        var request = await _uow.ReturnRequests.GetByIdAsync(returnRequestId, false, ct);
        if (request is null || request.OrderId != orderId || request.CustomerId != customer.Id) return AppResponse<ReturnRequestDto>.NotFound("Return request not found");
        return AppResponse<ReturnRequestDto>.Ok(Map(request, request.Order, request.Customer));
    }

    public async Task<AppResponse<List<ReturnRequestListItemDto>>> GetAdminReturnsAsync(ReturnRequestQueryDto query, CancellationToken ct)
    {
        query ??= new ReturnRequestQueryDto();
        if (query.Status.HasValue && query.Status is < 1 or > 3)
            return AppResponse<List<ReturnRequestListItemDto>>.ValidationError("Status must be 1 (pending), 2 (approved), or 3 (rejected)");
        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc >= query.ToUtc)
            return AppResponse<List<ReturnRequestListItemDto>>.ValidationError("FromUtc must be earlier than ToUtc");
        query.Page = Math.Max(1, query.Page); query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        int? pharmacyScope = null;
        if (!_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
        {
            var employee = await FindCurrentEmployeeAsync(ct);
            if (employee is null) return AppResponse<List<ReturnRequestListItemDto>>.Unauthorized("An active employee account is required");
            pharmacyScope = employee.PharmacyId;
        }
        var result = await _uow.ReturnRequests.SearchAsync(query, pharmacyScope, ct);
        return AppResponse<List<ReturnRequestListItemDto>>.Ok(result.Items, PaginationInfo.Create(query.Page, query.PageSize, result.TotalCount));
    }

    public async Task<AppResponse<ReturnRequestDto>> GetAdminReturnAsync(int returnRequestId, CancellationToken ct)
    {
        var request = await _uow.ReturnRequests.GetByIdAsync(returnRequestId, false, ct);
        if (request is null) return AppResponse<ReturnRequestDto>.NotFound("Return request not found");
        if (!_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
        {
            var employee = await FindCurrentEmployeeAsync(ct);
            if (employee is null) return AppResponse<ReturnRequestDto>.Unauthorized("An active employee account is required");
            if (employee.PharmacyId != request.Order.Store.PharmacyId)
                return AppResponse<ReturnRequestDto>.NotFound("Return request not found");
        }
        return AppResponse<ReturnRequestDto>.Ok(Map(request, request.Order, request.Customer));
    }

    public async Task<AppResponse<ReturnReviewResultDto>> ReviewAsync(int returnRequestId, ReviewReturnRequestDto dto, CancellationToken ct)
    {
        if (returnRequestId <= 0 || dto is null || dto.Notes?.Trim().Length > 500)
            return AppResponse<ReturnReviewResultDto>.ValidationError("Invalid review request");
        var employee = await FindCurrentEmployeeAsync(ct);
        if (employee is null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
            return AppResponse<ReturnReviewResultDto>.Unauthorized("An active employee account is required");
        await using var transaction = await _uow.BeginTransactionAsync(ct);
        try
        {
            await _uow.ReturnRequests.LockRequestForReviewAsync(returnRequestId, ct);
            var request = await _uow.ReturnRequests.GetByIdAsync(returnRequestId, true, ct);
            if (request is null) return AppResponse<ReturnReviewResultDto>.NotFound("Return request not found");
            if (request.Status != Pending) return AppResponse<ReturnReviewResultDto>.Conflict("Return request has already been reviewed");
            if (employee is not null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin)
                && employee.PharmacyId != request.Order.Store.PharmacyId)
                return AppResponse<ReturnReviewResultDto>.Forbidden("You cannot review returns for another pharmacy");
            await _uow.ReturnRequests.LockOrderForReturnsAsync(request.OrderId, ct);
            var now = DateTime.UtcNow;
            var result = new ReturnReviewResultDto { ReturnRequestId = request.Id, Status = dto.Approve ? Approved : Rejected };
            request.Status = result.Status;
            request.ReviewedByEmployeeId = employee?.Id;
            request.ReviewedByUserId = int.TryParse(_currentUser.UserId, out var reviewerUserId) ? reviewerUserId : null;
            request.ReviewedAt = now;
            request.ReviewNotes = dto.Notes?.Trim();
            request.UpdatedAt = now;
            if (dto.Approve)
            {
                if (request.Order.Status != 5)
                    return AppResponse<ReturnReviewResultDto>.BusinessRuleViolation("The order must remain delivered while its return is reviewed");
                var priorApproved = await _uow.ReturnRequests.GetApprovedItemsAsync(request.OrderId, request.Id, ct);
                foreach (var item in request.Items)
                    result.RestockedOuterQuantity += await _inventory.RestoreApprovedReturnItemAsync(item, ct);
                var points = await _points.ApplyApprovedReturnAsync(request.Order, request, priorApproved, ct);
                result.RedeemedPointsRefunded = points.RedeemedRefunded;
                result.EarnedPointsReversed = points.EarnedReversed;
            }
            await _uow.CompleteAsync(ct);
            await transaction.CommitAsync(ct);
            return AppResponse<ReturnReviewResultDto>.Ok(result, dto.Approve ? "Return approved" : "Return rejected");
        }
        catch (InventoryOperationException ex)
        {
            await transaction.RollbackAsync(ct);
            return AppResponse<ReturnReviewResultDto>.BusinessRuleViolation(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync(ct);
            return AppResponse<ReturnReviewResultDto>.BusinessRuleViolation(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<Customer?> FindCurrentCustomerAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId)) return null;
        return await _uow.Customers.FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, ct: ct);
    }

    private async Task<Employee?> FindCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId)) return null;
        return await _uow.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive && e.DeletedAt == null, ct: ct);
    }

    private static ReturnRequestDto Map(ReturnRequest request, Order order, Customer customer)
        => new()
        {
            Id = request.Id, OrderId = order.Id, OrderNumber = order.OrderNumber,
            CustomerId = customer.Id, CustomerNameAr = customer.FullNameAr, Status = request.Status,
            Reason = request.Reason, RequestedAt = request.RequestedAt, ReviewedByEmployeeId = request.ReviewedByEmployeeId,
            ReviewedByUserId = request.ReviewedByUserId,
            ReviewedAt = request.ReviewedAt, ReviewNotes = request.ReviewNotes,
            Items = request.Items.Select(i => new ReturnRequestItemDto
            {
                Id = i.Id, OrderItemId = i.OrderItemId, ProductId = i.OrderItem.ProductId,
                ProductNameAr = i.OrderItem.Product.NameAr, UnitLevel = i.OrderItem.UnitLevel,
                RequestedQuantity = i.Quantity, PurchasedQuantity = i.OrderItem.Quantity,
                FinalUnitPrice = i.OrderItem.FinalUnitPriceSnapshot,
                LineRefundValue = i.OrderItem.Quantity <= 0 ? 0 : decimal.Round(i.OrderItem.LineTotal * i.Quantity / i.OrderItem.Quantity, 2)
            }).ToList()
        };
}
