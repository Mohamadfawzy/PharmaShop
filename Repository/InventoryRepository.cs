using Contracts;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Inventory;

namespace Repository;

public sealed class InventoryRepository : IInventoryRepository
{
    private readonly RepositoryContext _db;
    public InventoryRepository(RepositoryContext db) => _db = db;

    public Task<Product?> GetProductAsync(int productId, CancellationToken ct)
        => _db.Products.Include(p => p.Store).FirstOrDefaultAsync(p => p.Id == productId && p.DeletedAt == null, ct);

    public Task<List<OrderItem>> GetOrderItemsAsync(int orderId, CancellationToken ct)
        => _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.Id).ToListAsync(ct);

    public Task<bool> HasOrderMovementAsync(int orderId, byte transactionType, CancellationToken ct)
        => _db.ProductInventoryTransactions.AnyAsync(x => x.OrderId == orderId && x.TransactionType == transactionType, ct);

    public Task<bool> HasReturnMovementAsync(int returnRequestItemId, CancellationToken ct)
        => _db.ProductInventoryTransactions.AnyAsync(x => x.ReturnRequestItemId == returnRequestItemId, ct);

    public Task<List<ProductInventoryTransaction>> GetOrderMovementsAsync(int orderId, byte transactionType, CancellationToken ct)
        => _db.ProductInventoryTransactions.Where(x => x.OrderId == orderId && x.TransactionType == transactionType)
            .OrderBy(x => x.Id).ToListAsync(ct);

    public async Task<bool> ApplyStockDeltaAsync(int productId, decimal expectedQuantity, decimal delta, CancellationToken ct)
    {
        var query = _db.Products.Where(p => p.Id == productId && p.DeletedAt == null && p.Quantity == expectedQuantity);
        if (delta < 0)
        {
            var amount = -delta;
            query = query.Where(p => p.Quantity >= amount);
            return await query.ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Quantity, p => p.Quantity - amount)
                .SetProperty(p => p.IsAvailable, p => p.Quantity - amount > 0)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct) == 1;
        }

        return await query.ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.Quantity, p => p.Quantity + delta)
            .SetProperty(p => p.IsAvailable, true)
            .SetProperty(p => p.UpdatedAt, DateTime.UtcNow), ct) == 1;
    }

    public Task AddMovementAsync(ProductInventoryTransaction movement, CancellationToken ct)
        => _db.ProductInventoryTransactions.AddAsync(movement, ct).AsTask();

    public async Task<(List<InventoryTransactionDto> Items, int TotalCount)> SearchAsync(InventoryTransactionQueryDto query, int? pharmacyId, CancellationToken ct)
    {
        var source = _db.ProductInventoryTransactions.AsNoTracking().AsQueryable();
        if (pharmacyId.HasValue) source = source.Where(x => x.Product.Store.PharmacyId == pharmacyId.Value);
        if (query.ProductId.HasValue) source = source.Where(x => x.ProductId == query.ProductId.Value);
        if (query.StoreId.HasValue) source = source.Where(x => x.Product.StoreId == query.StoreId.Value);
        if (query.EmployeeId.HasValue) source = source.Where(x => x.EmployeeId == query.EmployeeId.Value);
        if (query.OrderId.HasValue) source = source.Where(x => x.OrderId == query.OrderId.Value);
        if (query.TransactionType.HasValue) source = source.Where(x => x.TransactionType == query.TransactionType.Value);
        if (query.FromUtc.HasValue) source = source.Where(x => x.CreatedAt >= query.FromUtc.Value);
        if (query.ToUtc.HasValue) source = source.Where(x => x.CreatedAt < query.ToUtc.Value);
        var total = await source.CountAsync(ct);
        var items = await source.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new InventoryTransactionDto
            {
                Id = x.Id, ProductId = x.ProductId, ProductName = x.Product.NameAr,
                StoreId = x.Product.StoreId, EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee == null ? null : x.Employee.FullNameAr,
                OrderId = x.OrderId, OrderItemId = x.OrderItemId, TransactionType = x.TransactionType,
                ProductUnitId = x.ProductUnitId, BatchNumber = x.BatchNumber,
                ExpirationDate = x.ExpirationDate, CostPrice = x.CostPrice,
                Quantity = x.Quantity, QuantityBefore = x.QuantityBefore, QuantityAfter = x.QuantityAfter,
                Reason = x.Reason, CreatedAt = x.CreatedAt
            }).ToListAsync(ct);
        return (items, total);
    }
}
