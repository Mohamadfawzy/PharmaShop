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
        Promotions = new PromotionRepository(_context);
        Carts = new CartRepository(_context);
        Units = new GenericRepository<Unit>(_context);
        Tags = new GenericRepository<Tag>(_context);
        CartItems = new GenericRepository<CartItem>(_context);
    }

    public ICustomerRepository Customers { get; private set; }
    public IProductRepository Products { get; private set; }
    public ICategoryRepository Categories { get; private set; }
    public ICartRepository Carts { get; private set; }
    public IPromotionRepository Promotions { get; private set; }

    public IGenericRepository<Unit> Units { get; private set; }
    public IGenericRepository<CartItem> CartItems { get; private set; }

    public IGenericRepository<Tag> Tags { get; private set; }

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
