namespace Contracts;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    Task<int> CompleteAsync();

}
