using Entities.Models;

namespace Contracts;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IGenericRepository<ProductAuditLog> ProductAuditLogs { get; }
    void SetOriginalRowVersion<T>(T entity, byte[] rowVersion) where T : class;

    Task<int> CompleteAsync(CancellationToken ct = default);

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
