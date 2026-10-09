using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Common.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutResponseDto> CreateSessionAsync(
        Guid userId,
        CheckoutRequestDto? request,
        CancellationToken cancellationToken);
}
