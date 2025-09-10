using System;
using Contracts;

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
    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

}
