using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Services;

public sealed class CartService : ICartService
{
    private const int MaximumQuantity = 99;
    private const int MaximumDisplayedStock = 10;
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(Guid? userId, string? sessionId)
    {
        var cart = await FindCartAsync(userId, sessionId);
        return cart is null ? EmptyCart() : ToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid? userId, string? sessionId, Guid productId, int quantity)
    {
        ValidateQuantity(quantity);
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive);
        if (product is null)
            throw new CartNotFoundException("Product not found");

        var cart = await FindCartAsync(userId, sessionId);
        if (cart is null)
        {
            cart = new Cart { UserId = userId, SessionId = userId.HasValue ? null : sessionId };
            _context.Carts.Add(cart);
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        var newQuantity = (item?.Quantity ?? 0) + quantity;
        ValidateQuantity(newQuantity);

        if (newQuantity > product.StockQuantity)
            throw new CartConflictException($"Only {Math.Max(product.StockQuantity - (item?.Quantity ?? 0), 0)} left in stock");

        if (item is null)
        {
            item = new CartItem { Cart = cart, ProductId = productId, Product = product, Quantity = newQuantity };
            cart.Items.Add(item);
        }
        else
        {
            item.Quantity = newQuantity;
            item.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToDto(cart);
    }

    public async Task<CartDto> UpdateQuantityAsync(Guid? userId, string? sessionId, Guid productId, int quantity)
    {
        ValidateQuantity(quantity);
        var cart = await FindCartAsync(userId, sessionId);
        var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new CartNotFoundException("Item not in cart");

        if (quantity > item.Product.StockQuantity)
            throw new CartConflictException($"Only {Math.Max(item.Product.StockQuantity, 0)} left in stock");

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        cart!.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToDto(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid? userId, string? sessionId, Guid productId)
    {
        var cart = await FindCartAsync(userId, sessionId);
        var item = cart?.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new CartNotFoundException("Item not in cart");

        _context.CartItems.Remove(item);
        cart!.Items.Remove(item);
        cart!.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToDto(cart);
    }

    public async Task<CartDto> ClearCartAsync(Guid? userId, string? sessionId)
    {
        var cart = await FindCartAsync(userId, sessionId);
        if (cart is not null)
        {
            _context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return EmptyCart();
    }

    public async Task MergeGuestCartAsync(Guid userId, string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return;

        var guestCart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        if (guestCart is null)
            return;

        var userCart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (userCart is null)
        {
            guestCart.UserId = userId;
            guestCart.SessionId = null;
            guestCart.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            foreach (var guestItem in guestCart.Items.ToArray())
            {
                var userItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                if (userItem is null)
                {
                    guestItem.CartId = userCart.Id;
                    userCart.Items.Add(guestItem);
                }
                else
                {
                    userItem.Quantity = Math.Min(
                        userItem.Quantity + guestItem.Quantity,
                        Math.Max(guestItem.Product.StockQuantity, 0));
                    userItem.UpdatedAt = DateTime.UtcNow;
                    _context.CartItems.Remove(guestItem);
                }
            }

            _context.Carts.Remove(guestCart);
            userCart.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<Cart?> FindCartAsync(Guid? userId, string? sessionId)
    {
        var query = _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (userId.HasValue)
            return await query.Where(c => c.UserId == userId).OrderBy(c => c.CreatedAt).FirstOrDefaultAsync();

        return string.IsNullOrWhiteSpace(sessionId)
            ? null
            : await query.FirstOrDefaultAsync(c => c.SessionId == sessionId);
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > MaximumQuantity)
        {
            throw new InvalidRequestException(new Dictionary<string, string[]>
            {
                ["quantity"] = ["Quantity must be between 1 and 99."]
            });
        }
    }

    private static CartDto ToDto(Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Select(i => new CartItemDto(
                i.ProductId,
                i.Product.Title,
                i.Product.ImageUrl,
                i.Product.Price,
                i.Quantity,
                i.Product.Price * i.Quantity,
                Math.Clamp(i.Product.StockQuantity, 0, MaximumDisplayedStock),
                i.Product.IsActive && i.Product.StockQuantity > 0))
            .ToArray();

        return new CartDto(items, items.Sum(i => i.Quantity), items.Sum(i => i.LineTotal));
    }

    private static CartDto EmptyCart() => new(Array.Empty<CartItemDto>(), 0, 0m);
}