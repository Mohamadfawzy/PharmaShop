namespace Contracts;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }

    Task<int> CompleteAsync(CancellationToken ct = default);
}
