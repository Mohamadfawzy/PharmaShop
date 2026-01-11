using Contracts;
using Entities.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Dtos.Product;

namespace Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    private readonly RepositoryContext context;

    public ProductRepository(RepositoryContext context) : base(context)
    {
        this.context = context;
    }


    public Task<Product?> GetForUpdateAsync(int id, int pharmacyId, CancellationToken ct)
       => _dbSet.FirstOrDefaultAsync(p =>
           p.Id == id &&
           p.PharmacyId == pharmacyId &&
           p.DeletedAt == null, ct);

    public void SetRowVersionOriginalValue(Product entity, byte[] rowVersion)
    {
        _context.Entry(entity).Property(e => e.RowVersion).OriginalValue = rowVersion;
    }



}
