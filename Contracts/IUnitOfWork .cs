using Entities.Models;

namespace Contracts;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }

    IGenericRepository<Entities.Models.Unit> Units { get; }

    ICategoryRepository Categories { get; }
    void SetOriginalRowVersion<T>(T entity, byte[] rowVersion) where T : class;

    Task<int> CompleteAsync(CancellationToken ct = default);

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
