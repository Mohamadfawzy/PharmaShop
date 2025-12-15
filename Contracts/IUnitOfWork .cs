namespace Contracts;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }

    Task<int> CompleteAsync(CancellationToken ct = default);

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
