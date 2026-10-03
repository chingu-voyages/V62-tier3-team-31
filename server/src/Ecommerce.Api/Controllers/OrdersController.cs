using System.Globalization;
using System.Security.Claims;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultLimit = 10;
    private const int MaximumLimit = 50;

    /// <summary>Lists the signed-in user's paid orders, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResponseDto<OrderSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponseDto<OrderSummaryDto>>> GetOrders(
        [FromQuery] string? page,
        [FromQuery] string? limit,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        var parsedPage = ParseInteger(page, "page", DefaultPage, value => value >= 1, "Page must be 1 or greater.", errors);
        var parsedLimit = ParseInteger(limit, "limit", DefaultLimit, value => value is >= 1 and <= MaximumLimit, "Limit must be between 1 and 50.", errors);

        if (errors.Count > 0)
            throw new InvalidRequestException(errors);

        if (!TryGetUserId(out var userId))
            return Unauthorized(new { title = "Not logged in", status = StatusCodes.Status401Unauthorized });

        var orders = await orderService.GetOrdersAsync(userId, parsedPage, parsedLimit, cancellationToken);
        return Ok(orders);
    }

    /// <summary>Returns the full details of one of the signed-in user's orders.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var orderId))
        {
            throw new InvalidRequestException(new Dictionary<string, string[]>
            {
                ["id"] = ["ID must be a valid UUID."]
            });
        }

        if (!TryGetUserId(out var userId))
            return Unauthorized(new { title = "Not logged in", status = StatusCodes.Status401Unauthorized });

        var order = await orderService.GetOrderAsync(userId, orderId, cancellationToken);
        return order is null
            ? NotFound(new { title = "Order not found", status = StatusCodes.Status404NotFound })
            : Ok(order);
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    private static int ParseInteger(
        string? rawValue,
        string field,
        int defaultValue,
        Func<int, bool> isValid,
        string errorMessage,
        Dictionary<string, string[]> errors)
    {
        if (rawValue is null)
            return defaultValue;

        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            errors[field] = [errorMessage];
            return defaultValue;
        }

        if (!isValid(value))
            errors[field] = [errorMessage];

        return value;
    }
}
