using System.Security.Claims;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/checkout")]
public sealed class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkout;

    public CheckoutController(ICheckoutService checkout)
    {
        _checkout = checkout;
    }

    [HttpPost("session")]
    public async Task<IActionResult> CreateSession(
        [FromBody] CheckoutRequestDto? request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized(new { title = "Not logged in", status = StatusCodes.Status401Unauthorized });

        var result = await _checkout.CreateSessionAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
