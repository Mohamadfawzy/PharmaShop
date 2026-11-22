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
        Pharmacies = new GenericRepository<Pharmacy>(_context);
        Stores = new GenericRepository<Store>(_context);
        Employees = new GenericRepository<Employee>(_context);
        Users = new GenericRepository<AspNetUser>(_context);
        UserNotifications = new GenericRepository<UserNotification>(_context);
        NotificationPreferences = new GenericRepository<CustomerNotificationPreference>(_context);
        PaymentAttempts = new GenericRepository<PaymentAttempt>(_context);
        PaymentRefunds = new GenericRepository<PaymentRefund>(_context);
        Governorates = new GenericRepository<Governorate>(_context);
        GovernorateAreas = new GenericRepository<GovernorateArea>(_context);
        DeliveryZones = new GenericRepository<DeliveryZone>(_context);
        DeliveryZoneAreas = new GenericRepository<DeliveryZoneArea>(_context);
        CustomerAddresses = new GenericRepository<CustomerAddress>(_context);
        Products = new ProductRepository(_context);
        Categories = new CategoryRepository(_context);
        Promotions = new PromotionRepository(_context);
        Carts = new CartRepository(_context);
        Units = new GenericRepository<Unit>(_context);
        Companies = new GenericRepository<Company>(_context);
        Tags = new GenericRepository<Tag>(_context);
        ProductImages = new GenericRepository<ProductImage>(_context);
        ProductTags = new GenericRepository<ProductTag>(_context);
        Banners = new GenericRepository<PromotionalBanner>(_context);
        CartItems = new GenericRepository<CartItem>(_context);
        Orders = new OrderRepository(_context);
        Points = new PointsRepository(_context);
        Prescriptions = new PrescriptionRepository(_context);
        Inventory = new InventoryRepository(_context);
        ReturnRequests = new ReturnRequestRepository(_context);
    }

    public ICustomerRepository Customers { get; private set; }
    public IGenericRepository<Pharmacy> Pharmacies { get; private set; }
    public IGenericRepository<Store> Stores { get; private set; }
    public IGenericRepository<Employee> Employees { get; private set; }
    public IGenericRepository<AspNetUser> Users { get; private set; }
    public IGenericRepository<UserNotification> UserNotifications { get; private set; }
    public IGenericRepository<CustomerNotificationPreference> NotificationPreferences { get; private set; }
    public IGenericRepository<PaymentAttempt> PaymentAttempts { get; private set; }
    public IGenericRepository<PaymentRefund> PaymentRefunds { get; private set; }
    public IGenericRepository<Governorate> Governorates { get; private set; }
    public IGenericRepository<GovernorateArea> GovernorateAreas { get; private set; }
    public IGenericRepository<DeliveryZone> DeliveryZones { get; private set; }
    public IGenericRepository<DeliveryZoneArea> DeliveryZoneAreas { get; private set; }
    public IGenericRepository<CustomerAddress> CustomerAddresses { get; private set; }
    public IProductRepository Products { get; private set; }
    public ICategoryRepository Categories { get; private set; }
    public ICartRepository Carts { get; private set; }
    public IPromotionRepository Promotions { get; private set; }
    public IOrderRepository Orders { get; private set; }
    public IPointsRepository Points { get; private set; }
    public IPrescriptionRepository Prescriptions { get; private set; }
    public IInventoryRepository Inventory { get; private set; }
    public IReturnRequestRepository ReturnRequests { get; private set; }

    public IGenericRepository<Unit> Units { get; private set; }
    public IGenericRepository<Company> Companies { get; private set; }
    public IGenericRepository<CartItem> CartItems { get; private set; }

    public IGenericRepository<Tag> Tags { get; private set; }
    public IGenericRepository<ProductImage> ProductImages { get; private set; }
    public IGenericRepository<ProductTag> ProductTags { get; private set; }
    public IGenericRepository<PromotionalBanner> Banners { get; private set; }

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
