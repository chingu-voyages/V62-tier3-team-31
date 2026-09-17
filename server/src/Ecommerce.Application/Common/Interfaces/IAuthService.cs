using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.Application;

public interface IAuthService
{
    Task<(bool Success, string ErrorMessage, AuthResponse? Response)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string ErrorMessage, AuthResponse? Response)> LoginAsync(LoginRequest request);
}
