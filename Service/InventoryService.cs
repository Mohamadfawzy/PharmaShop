using Contracts;
using Contracts.IServices;
using Entities.Models;
using Shared.Models.Dtos.Inventory;
using Shared.Models.Dtos.Product;
using Shared.Responses;
using FluentValidation;

namespace Service;

public sealed class InventoryService : IInventoryService
{
    private const byte Receipt = 1, AdjustmentIncrease = 2, AdjustmentDecrease = 3, OrderDeduction = 4, OrderRestock = 5, ReturnRestock = 6;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ReceiveStockDto> _receiveValidator;

    public InventoryService(IUnitOfWork uow, ICurrentUserService currentUser, IValidator<ReceiveStockDto> receiveValidator)
    {
        _uow = uow;
        _currentUser = currentUser;
        _receiveValidator = receiveValidator;
    }

    public async Task<AppResponse<InventoryTransactionDto>> ReceiveAsync(int productId, ReceiveStockDto dto, CancellationToken ct)
    {
        if (productId <= 0 || dto is null) return AppResponse<InventoryTransactionDto>.ValidationError("ProductId and request body are required");
        var validation = await _receiveValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
            return AppResponse<InventoryTransactionDto>.ValidationErrors(errors, detail: "Validation failed");
        }
        var employee = await GetCurrentEmployeeAsync(ct);
        if (employee is null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
            return AppResponse<InventoryTransactionDto>.Unauthorized("An active employee account is required");
        var product = await _uow.Inventory.GetProductAsync(productId, ct);
        if (product is null) return AppResponse<InventoryTransactionDto>.NotFound("Product not found");
        if (product.StoreId != dto.StoreId || employee is not null && product.Store.PharmacyId != employee.PharmacyId)
            return AppResponse<InventoryTransactionDto>.Forbidden("You cannot receive stock for this store");
        if (dto.ProductUnitId != product.OuterUnitId && dto.ProductUnitId != product.InnerUnitId)
            return AppResponse<InventoryTransactionDto>.ValidationError("ProductUnitId must match the product outer or inner unit");
        if (product.HasExpiry && !dto.ExpirationDate.HasValue)
            return AppResponse<InventoryTransactionDto>.ValidationError("ExpirationDate is required for products with expiry tracking");
        var isInnerUnit = dto.ProductUnitId == product.InnerUnitId && dto.ProductUnitId != product.OuterUnitId;
        if (isInnerUnit && product.InnerPerOuter.GetValueOrDefault() <= 0)
            return AppResponse<InventoryTransactionDto>.ValidationError("Product inner-unit conversion is not configured");

        var receivedOuterQuantity = isInnerUnit
            ? dto.Quantity / product.InnerPerOuter!.Value : dto.Quantity;
        var reason = string.IsNullOrWhiteSpace(dto.Reason) ? $"Stock received, batch {dto.BatchNumber}" : dto.Reason;
        return await ManualMovementAsync(productId, receivedOuterQuantity, reason, Receipt, ct,
            dto.ProductUnitId, dto.BatchNumber, dto.ExpirationDate, dto.CostPrice, employee);
    }

    public Task<AppResponse<InventoryTransactionDto>> AdjustAsync(AdjustInventoryDto dto, CancellationToken ct)
    {
        if (dto is null || dto.QuantityDelta == 0)
            return Task.FromResult(AppResponse<InventoryTransactionDto>.ValidationError("QuantityDelta must be non-zero"));
        return ManualMovementAsync(dto.ProductId, dto.QuantityDelta, dto.Reason,
            dto.QuantityDelta > 0 ? AdjustmentIncrease : AdjustmentDecrease, ct);
    }

    private async Task<AppResponse<InventoryTransactionDto>> ManualMovementAsync(int productId, decimal delta, string? reason, byte type, CancellationToken ct,
        int? productUnitId = null, string? batchNumber = null, DateTime? expirationDate = null, decimal? costPrice = null, Employee? knownEmployee = null)
    {
        if (productId <= 0 || delta <= 0 && type == Receipt || delta == 0 || Math.Abs(delta) > 1_000_000m)
            return AppResponse<InventoryTransactionDto>.ValidationError("ProductId and quantity are invalid");
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
            return AppResponse<InventoryTransactionDto>.ValidationError("Reason is required and must be at most 500 characters");
        var employee = knownEmployee ?? await GetCurrentEmployeeAsync(ct);
        if (employee is null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
            return AppResponse<InventoryTransactionDto>.Unauthorized("An active employee account is required");
        var product = await _uow.Inventory.GetProductAsync(productId, ct);
        if (product is null) return AppResponse<InventoryTransactionDto>.NotFound("Product not found");
        if (employee is not null && employee.PharmacyId != product.Store.PharmacyId)
            return AppResponse<InventoryTransactionDto>.Forbidden("You cannot manage inventory outside your pharmacy");

        var before = product.Quantity;
        var movementDelta = delta;
        await using var tx = await _uow.BeginTransactionAsync(ct);
        if (!await _uow.Inventory.ApplyStockDeltaAsync(productId, before, movementDelta, ct))
        {
            await tx.RollbackAsync(ct);
            return movementDelta < 0
                ? AppResponse<InventoryTransactionDto>.BusinessRuleViolation("Insufficient stock or stock changed; refresh and retry")
                : AppResponse<InventoryTransactionDto>.Conflict("Stock changed; refresh and retry");
        }
        var movement = new ProductInventoryTransaction
        {
            ProductId = productId, EmployeeId = employee?.Id, TransactionType = type,
            ProductUnitId = productUnitId ?? product.OuterUnitId, BatchNumber = batchNumber?.Trim(), ExpirationDate = expirationDate,
            CostPrice = costPrice,
            Quantity = Math.Abs(movementDelta), QuantityBefore = before, QuantityAfter = before + movementDelta,
            Reason = ActorReason(reason.Trim()), CreatedAt = DateTime.UtcNow
        };
        await _uow.Inventory.AddMovementAsync(movement, ct);
        await _uow.CompleteAsync(ct);
        await tx.CommitAsync(ct);
        return AppResponse<InventoryTransactionDto>.Ok(ToDto(movement, product.NameAr, product.StoreId, employee?.FullNameAr), "Inventory updated successfully");
    }

    public async Task<AppResponse<InventoryTransactionPageDto>> SearchAsync(InventoryTransactionQueryDto query, CancellationToken ct)
    {
        query ??= new InventoryTransactionQueryDto();
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        if (query.FromUtc.HasValue && query.ToUtc.HasValue && query.FromUtc >= query.ToUtc)
            return AppResponse<InventoryTransactionPageDto>.ValidationError("FromUtc must be earlier than ToUtc");
        int? pharmacyScope = null;
        if (!_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
        {
            var employee = await GetCurrentEmployeeAsync(ct);
            if (employee is null) return AppResponse<InventoryTransactionPageDto>.Unauthorized("An active employee account is required");
            pharmacyScope = employee.PharmacyId;
        }
        var result = await _uow.Inventory.SearchAsync(query, pharmacyScope, ct);
        return AppResponse<InventoryTransactionPageDto>.Ok(new InventoryTransactionPageDto
        {
            Items = result.Items, Page = query.Page, PageSize = query.PageSize, TotalCount = result.TotalCount
        });
    }

    public async Task ApplyOrderConfirmationAsync(int orderId, CancellationToken ct)
    {
        var employee = await GetCurrentEmployeeAsync(ct);
        if (employee is null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
            throw new InventoryOperationException("Active employee account is required to confirm an order");
        if (await _uow.Inventory.HasOrderMovementAsync(orderId, OrderDeduction, ct)) return;
        var items = await _uow.Inventory.GetOrderItemsAsync(orderId, ct);
        var products = items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(ToOuterQuantity));
        foreach (var pair in products)
        {
            var product = await _uow.Inventory.GetProductAsync(pair.Key, ct)
                ?? throw new InventoryOperationException($"Product {pair.Key} no longer exists");
            if (employee is not null && product.Store.PharmacyId != employee.PharmacyId)
                throw new InventoryOperationException("You cannot confirm an order for another pharmacy");
            if (!product.IsActive || !product.IsAvailable)
                throw new InventoryOperationException($"Product {product.Id} is not currently available");
            var before = product.Quantity;
            if (before < pair.Value || !await _uow.Inventory.ApplyStockDeltaAsync(product.Id, before, -pair.Value, ct))
                throw new InventoryOperationException($"Insufficient stock for product {product.Id}");
            foreach (var line in items.Where(i => i.ProductId == pair.Key))
            {
                var quantity = ToOuterQuantity(line);
                var currentBefore = before - items.Where(i => i.ProductId == pair.Key && i.Id < line.Id).Sum(ToOuterQuantity);
                await _uow.Inventory.AddMovementAsync(new ProductInventoryTransaction
                {
                    ProductId = line.ProductId, EmployeeId = employee?.Id, OrderId = orderId, OrderItemId = line.Id,
                    TransactionType = OrderDeduction, Quantity = quantity, QuantityBefore = currentBefore,
                    QuantityAfter = currentBefore - quantity, Reason = ActorReason($"Order confirmation #{orderId}"), CreatedAt = DateTime.UtcNow
                }, ct);
            }
        }
    }

    public async Task RestoreOrderStockAsync(int orderId, CancellationToken ct)
    {
        var deductions = await _uow.Inventory.GetOrderMovementsAsync(orderId, OrderDeduction, ct);
        if (deductions.Count == 0 || await _uow.Inventory.HasOrderMovementAsync(orderId, OrderRestock, ct)) return;
        var employee = await GetCurrentEmployeeAsync(ct);
        if (employee is null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin) && !_currentUser.IsInRole(Shared.Constants.AppRoles.Customer))
            throw new InventoryOperationException("Active employee account is required to cancel or return an order");
        foreach (var group in deductions.GroupBy(x => x.ProductId))
        {
            var product = await _uow.Inventory.GetProductAsync(group.Key, ct)
                ?? throw new InventoryOperationException($"Product {group.Key} no longer exists");
            if (employee is not null && product.Store.PharmacyId != employee.PharmacyId)
                throw new InventoryOperationException("You cannot restore stock for another pharmacy");
            var before = product.Quantity;
            var totalToRestore = group.Sum(x => x.Quantity);
            if (!await _uow.Inventory.ApplyStockDeltaAsync(product.Id, before, totalToRestore, ct))
                throw new InventoryOperationException($"Stock changed while restoring product {product.Id}");
            var restoredSoFar = 0m;
            foreach (var deduction in group)
            {
                await _uow.Inventory.AddMovementAsync(new ProductInventoryTransaction
                {
                    ProductId = deduction.ProductId, EmployeeId = employee?.Id, OrderId = orderId, OrderItemId = deduction.OrderItemId,
                    TransactionType = OrderRestock, Quantity = deduction.Quantity, QuantityBefore = before + restoredSoFar,
                    QuantityAfter = before + restoredSoFar + deduction.Quantity,
                    Reason = ActorReason($"Stock restored for cancelled/returned order #{orderId}"), CreatedAt = DateTime.UtcNow
                }, ct);
                restoredSoFar += deduction.Quantity;
            }
        }
    }

    public async Task<decimal> RestoreApprovedReturnItemAsync(ReturnRequestItem item, CancellationToken ct)
    {
        if (await _uow.Inventory.HasReturnMovementAsync(item.Id, ct)) return 0m;
        var employee = await GetCurrentEmployeeAsync(ct);
        if (employee is null && !_currentUser.IsInRole(Shared.Constants.AppRoles.Admin))
            throw new InventoryOperationException("An active employee account is required to approve returns");
        var orderItem = item.OrderItem;
        var outerQuantity = orderItem.UnitLevel == 2
            ? item.Quantity / (orderItem.InnerPerOuterSnapshot.GetValueOrDefault() > 0
                ? orderItem.InnerPerOuterSnapshot!.Value
                : throw new InventoryOperationException($"Order item {orderItem.Id} has no inner-unit snapshot"))
            : item.Quantity;
        var product = await _uow.Inventory.GetProductAsync(orderItem.ProductId, ct)
            ?? throw new InventoryOperationException($"Product {orderItem.ProductId} no longer exists");
        if (employee is not null && employee.PharmacyId != product.Store.PharmacyId)
            throw new InventoryOperationException("You cannot approve a return for another pharmacy");
        var before = product.Quantity;
        if (!await _uow.Inventory.ApplyStockDeltaAsync(product.Id, before, outerQuantity, ct))
            throw new InventoryOperationException($"Stock changed while restoring product {product.Id}");
        await _uow.Inventory.AddMovementAsync(new ProductInventoryTransaction
        {
            ProductId = product.Id, EmployeeId = employee?.Id, OrderId = item.ReturnRequest.OrderId,
            OrderItemId = orderItem.Id, ReturnRequestItemId = item.Id, TransactionType = ReturnRestock,
            ProductUnitId = orderItem.UnitLevel == 2 ? orderItem.InnerUnitIdSnapshot : orderItem.OuterUnitIdSnapshot,
            Quantity = outerQuantity, QuantityBefore = before, QuantityAfter = before + outerQuantity,
            Reason = ActorReason($"Approved return request #{item.ReturnRequestId}"), CreatedAt = DateTime.UtcNow
        }, ct);
        return outerQuantity;
    }

    private async Task<Employee?> GetCurrentEmployeeAsync(CancellationToken ct)
    {
        if (!int.TryParse(_currentUser.UserId, out var userId)) return null;
        return await _uow.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.IsActive && e.DeletedAt == null, asNoTracking: true, ct: ct);
    }

    private string ActorReason(string reason)
        => _currentUser.IsInRole(Shared.Constants.AppRoles.Admin)
            ? $"Admin {_currentUser.UserId}: {reason}"
            : _currentUser.IsInRole(Shared.Constants.AppRoles.Customer)
                ? $"Customer {_currentUser.UserId}: {reason}"
            : reason;

    private static decimal ToOuterQuantity(OrderItem item)
        => item.UnitLevel == 2
            ? item.InnerPerOuterSnapshot.GetValueOrDefault() > 0 ? item.Quantity / item.InnerPerOuterSnapshot!.Value : throw new InventoryOperationException($"Order item {item.Id} has no valid inner-unit snapshot")
            : item.Quantity;

    private static InventoryTransactionDto ToDto(ProductInventoryTransaction movement, string productName, int storeId, string? employeeName)
        => new()
        {
            Id = movement.Id, ProductId = movement.ProductId, ProductName = productName, StoreId = storeId,
            EmployeeId = movement.EmployeeId, EmployeeName = employeeName, OrderId = movement.OrderId,
            OrderItemId = movement.OrderItemId, TransactionType = movement.TransactionType, ProductUnitId = movement.ProductUnitId,
            BatchNumber = movement.BatchNumber, ExpirationDate = movement.ExpirationDate, CostPrice = movement.CostPrice, Quantity = movement.Quantity,
            QuantityBefore = movement.QuantityBefore, QuantityAfter = movement.QuantityAfter,
            Reason = movement.Reason, CreatedAt = movement.CreatedAt
        };
}

public sealed class InventoryOperationException : Exception
{
    public InventoryOperationException(string message) : base(message) { }
}
