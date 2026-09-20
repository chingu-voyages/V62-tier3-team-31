using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Common.Interfaces;

public interface IAuthService
{
    Task<(UserDto User, string AccessToken, string RefreshToken)> RegisterAsync(RegisterRequestDto request);
    Task<(UserDto User, string AccessToken, string RefreshToken)> LoginAsync(LoginRequestDto request);

    Task<UserDto?> GetUserByIdAsync(Guid userId);
      Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);
}
