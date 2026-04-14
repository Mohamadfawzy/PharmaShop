using Shared.Models.Dtos.Cart;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;

public interface ICartService
{
    Task<AppResponse<AddToCartResponseDto>> AddItemAsync(CartAddItemDto dto, CancellationToken ct);
    Task<AppResponse<int>> ClearCartAsync(int customerId, CancellationToken ct);
    Task<AppResponse<CartViewDto>> GetMyCartAsync(int customerId, CancellationToken ct);
    Task<AppResponse<CartPreviewResponseDto>> PreviewAsync(CartPreviewRequestDto dto, CancellationToken ct);
    Task<AppResponse<int>> RemoveItemAsync(int cartItemId, int customerId, CancellationToken ct);
    Task<AppResponse<int>> UpdateItemQtyAsync(int cartItemId, CartUpdateQtyDto dto, CancellationToken ct);
}
