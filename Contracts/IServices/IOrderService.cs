using Shared.Models.Dtos.Order;
using Shared.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.IServices;

public interface IOrderService
{
    Task<AppResponse<CheckoutResultDto>> CheckoutAsync(CheckoutRequestDto dto, CancellationToken ct);
    Task<AppResponse<CheckoutPreviewResponseDto>> PreviewAsync(CheckoutPreviewRequestDto dto, CancellationToken ct);
}
