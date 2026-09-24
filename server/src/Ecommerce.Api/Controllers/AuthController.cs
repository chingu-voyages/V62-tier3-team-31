using System.Security.Claims;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Constants.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        SetAuthCookies(result.AccessToken, result.RefreshToken);
        return StatusCode(StatusCodes.Status201Created, new { user = result.User });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        SetAuthCookies(result.AccessToken, result.RefreshToken);
        return Ok(new { user = result.User });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies[AuthCookieNames.RefreshToken];
        var accessToken = await _authService.RefreshTokenAsync(refreshToken ?? string.Empty);
        Response.Cookies.Append(AuthCookieNames.AccessToken, accessToken, CreateCookieOptions("/", TimeSpan.FromMinutes(15)));
        return NoContent();
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(AuthCookieNames.AccessToken, DeleteCookieOptions("/"));
        Response.Cookies.Delete(AuthCookieNames.RefreshToken, DeleteCookieOptions("/api/v1/auth/refresh"));
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { title = "Not logged in", status = 401 });
        }

        var user = await _authService.GetUserByIdAsync(userId);
        return user is null
            ? Unauthorized(new { title = "Not logged in", status = 401 })
            : Ok(new { user });
    }

    private void SetAuthCookies(string accessToken, string refreshToken)
    {
        Response.Cookies.Append(AuthCookieNames.AccessToken, accessToken, CreateCookieOptions("/", TimeSpan.FromMinutes(15)));
        Response.Cookies.Append(AuthCookieNames.RefreshToken, refreshToken, CreateCookieOptions("/api/v1/auth/refresh", TimeSpan.FromDays(7)));
    }

    private static CookieOptions CreateCookieOptions(string path, TimeSpan lifetime) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = DateTimeOffset.UtcNow.Add(lifetime),
        Path = path
    };

    private static CookieOptions DeleteCookieOptions(string path) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Path = path
    };
}
