using Entities.Models;
using Shared.Enums.Cart;
using Shared.Models.Dtos.Cart;

namespace Contracts;


public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetActiveByCustomerAsync(int customerId, CancellationToken ct);
    Task<CartItem?> GetByCartProductUnitAsync(int cartId, int productId, UnitLevel unitLevel, CancellationToken ct);
    Task<CartItem?> GetCartItemForUpdateAsync(int cartItemId, int customerId, CancellationToken ct);
    Task<List<CartItem>> GetCartItemsByCartIdAsync(int cartId, CancellationToken ct);
    Task<Cart?> GetLatestCartByCustomerAsync(int customerId, CancellationToken ct);
    Task<CartViewDto> GetMyCartAsync(int customerId, CancellationToken ct);
    void RemoveCartItem(CartItem item);
    void RemoveCartItemsRange(IEnumerable<CartItem> items);
}

