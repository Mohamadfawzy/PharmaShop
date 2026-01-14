using Contracts;
using Entities.Models;
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
        Categories = new CategoryRepository(_context);
        ProductAuditLogs = new GenericRepository<ProductAuditLog>(_context);
        Units = new GenericRepository<Unit>(_context);
        ProductUnits = new GenericRepository<ProductUnit>(_context);

    }

    public ICustomerRepository Customers { get; private set; }
    public IProductRepository Products { get; private set; }
    public ICategoryRepository Categories { get; private set; }
    public IGenericRepository<ProductAuditLog> ProductAuditLogs { get; }

    public IGenericRepository<Unit> Units { get; private set; }

    public IGenericRepository<ProductUnit> ProductUnits { get; private set; }



    // METHODS
    public void SetOriginalRowVersion<T>(T entity, byte[] rowVersion) where T : class
    {
        _context.Entry(entity).Property("RowVersion").OriginalValue = rowVersion;
    }
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
