using System.Security.Cryptography;
using System.Security.Claims;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Constants.Api;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/v1/cart")]
public sealed class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var owner = ResolveOwner();
        return Ok(await _cartService.GetCartAsync(owner.UserId, owner.SessionId));
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequestDto? request)
    {
        var owner = ResolveOwner();
        var errors = new Dictionary<string, string[]>();
        if (request?.ProductId is null || request.ProductId == Guid.Empty)
            errors["productId"] = ["Product ID is required and must be a UUID."];
        if (request?.Quantity is null or < 1 or > 99)
            errors["quantity"] = ["Quantity must be between 1 and 99."];
        if (errors.Count > 0)
            throw new InvalidRequestException(errors);

        return Ok(await _cartService.AddItemAsync(owner.UserId, owner.SessionId, request!.ProductId!.Value, request.Quantity!.Value));
    }

    [HttpPatch("items/{productId}")]
    public async Task<IActionResult> UpdateQuantity(Guid productId, [FromBody] UpdateCartItemRequestDto? request)
    {
        var owner = ResolveOwner();
        if (productId == Guid.Empty)
            throw new InvalidRequestException(new Dictionary<string, string[]> { ["productId"] = ["Product ID must be a UUID."] });
        if (request?.Quantity is null or < 1 or > 99)
            throw new InvalidRequestException(new Dictionary<string, string[]> { ["quantity"] = ["Quantity must be between 1 and 99."] });

        return Ok(await _cartService.UpdateQuantityAsync(owner.UserId, owner.SessionId, productId, request!.Quantity!.Value));
    }

    [HttpDelete("items/{productId}")]
    public async Task<IActionResult> RemoveItem(Guid productId)
    {
        var owner = ResolveOwner();
        if (productId == Guid.Empty)
            throw new InvalidRequestException(new Dictionary<string, string[]> { ["productId"] = ["Product ID must be a UUID."] });

        return Ok(await _cartService.RemoveItemAsync(owner.UserId, owner.SessionId, productId));
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var owner = ResolveOwner();
        return Ok(await _cartService.ClearCartAsync(owner.UserId, owner.SessionId));
    }

    private (Guid? UserId, string? SessionId) ResolveOwner()
    {
        if (User.Identity?.IsAuthenticated == true &&
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return (userId, null);
        }

        if (!Request.Cookies.TryGetValue(CartCookieNames.Session, out var sessionId) || string.IsNullOrWhiteSpace(sessionId))
        {
            sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            Response.Cookies.Append(CartCookieNames.Session, sessionId, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                Path = "/"
            });
        }

        return (null, sessionId);
    }
}