using Entities.Models;

namespace Contracts;

public interface IUnitOfWork : IDisposable
{
    ICustomerRepository Customers { get; }
    IGenericRepository<Pharmacy> Pharmacies { get; }
    IGenericRepository<Store> Stores { get; }
    IGenericRepository<Employee> Employees { get; }
    IGenericRepository<Governorate> Governorates { get; }
    IGenericRepository<GovernorateArea> GovernorateAreas { get; }
    IGenericRepository<DeliveryZone> DeliveryZones { get; }
    IGenericRepository<DeliveryZoneArea> DeliveryZoneAreas { get; }
    IGenericRepository<CustomerAddress> CustomerAddresses { get; }
    IProductRepository Products { get; }

    IGenericRepository<Entities.Models.Unit> Units { get; }
    IGenericRepository<Company> Companies { get; }
    IGenericRepository<Tag> Tags { get; }
    IGenericRepository<ProductImage> ProductImages { get; }
    IGenericRepository<ProductTag> ProductTags { get; }
    IGenericRepository<CartItem> CartItems { get; }

    ICategoryRepository Categories { get; }
    ICartRepository Carts { get; }
    IPromotionRepository Promotions { get; }
    IOrderRepository Orders { get; }
    IPointsRepository Points { get; }
    IPrescriptionRepository Prescriptions { get; }
    IInventoryRepository Inventory { get; }

    void SetOriginalRowVersion<T>(T entity, byte[] rowVersion) where T : class;

    Task<int> CompleteAsync(CancellationToken ct = default);

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
    IReturnRequestRepository ReturnRequests { get; }
}
