using Contracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly RepositoryContext _context;

    public UnitOfWork(RepositoryContext context)
    {
        _context = context;
        Customers = new CustomerRepository(_context);
        Products = new ProductRepository(_context);
    }

    public ICustomerRepository Customers { get; private set; }
    public IProductRepository Products { get; private set; }
    
    // METHODS
    public async Task<int> CompleteAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }


    public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var dbTransaction = await _context.Database.BeginTransactionAsync(ct);
        return new EfTransaction(dbTransaction);
    }
}
