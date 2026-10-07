using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Common.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid? userId, string? sessionId);
    Task<CartDto> AddItemAsync(Guid? userId, string? sessionId, Guid productId, int quantity);
    Task<CartDto> UpdateQuantityAsync(Guid? userId, string? sessionId, Guid productId, int quantity);
    Task<CartDto> RemoveItemAsync(Guid? userId, string? sessionId, Guid productId);
    Task<CartDto> ClearCartAsync(Guid? userId, string? sessionId);
    Task MergeGuestCartAsync(Guid userId, string? sessionId);
}