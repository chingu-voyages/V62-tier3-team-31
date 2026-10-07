using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Common.Interfaces;

public interface IOrderService
{
    Task<PagedResponseDto<OrderSummaryDto>> GetOrdersAsync(
        Guid userId,
        int page,
        int limit,
        CancellationToken cancellationToken);

    Task<OrderDto?> GetOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken);
}
