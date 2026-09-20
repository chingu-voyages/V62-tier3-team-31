using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task<(UserDto User, string AccessToken, string RefreshToken)> RegisterAsync(RegisterRequestDto request)
    {
        ValidateRegistration(request);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email == normalizedEmail))
        {
            throw new DuplicateEmailException();
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return (
            ToDto(user),
            GenerateToken(user.Id, isRefreshToken: false),
            GenerateToken(user.Id, isRefreshToken: true));
    }

    public async Task<(UserDto User, string AccessToken, string RefreshToken)> LoginAsync(LoginRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Email))
            errors["email"] = ["Email is required."];
        if (string.IsNullOrEmpty(request.Password))
            errors["password"] = ["Password is required."];
        if (errors.Count > 0)
            throw new InvalidRequestException(errors);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        return (
            ToDto(user),
            GenerateToken(user.Id, isRefreshToken: false),
            GenerateToken(user.Id, isRefreshToken: true));
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Please log in again");

        try
        {
            var settings = _configuration.GetSection("JwtSettings");
            var secret = settings["RefreshTokenSecret"]
                ?? throw new InvalidOperationException("JWT refresh-token secret is missing in configuration.");
            var issuer = settings["Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is missing in configuration.");
            var audience = settings["Audience"]
                ?? throw new InvalidOperationException("JWT audience is missing in configuration.");

            var principal = new JwtSecurityTokenHandler().ValidateToken(
                refreshToken,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out _);

            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userIdValue, out var userId))
                throw new UnauthorizedAccessException("Please log in again");

            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                throw new UnauthorizedAccessException("Please log in again");

            return (
                GenerateToken(user.Id, isRefreshToken: false),
                GenerateToken(user.Id, isRefreshToken: true));
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedAccessException("Please log in again");
        }
        catch (ArgumentException)
        {
            throw new UnauthorizedAccessException("Please log in again");
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user is null ? null : ToDto(user);
    }

    private string GenerateToken(Guid userId, bool isRefreshToken)
    {
        var settings = _configuration.GetSection("JwtSettings");
        var secretName = isRefreshToken ? "RefreshTokenSecret" : "AccessTokenSecret";
        var secret = settings[secretName]
            ?? throw new InvalidOperationException($"JWT {secretName} is missing in configuration.");
        var issuer = settings["Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is missing in configuration.");
        var audience = settings["Audience"]
            ?? throw new InvalidOperationException("JWT audience is missing in configuration.");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var expires = isRefreshToken
            ? DateTime.UtcNow.AddDays(7)
            : DateTime.UtcNow.AddMinutes(15);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }

    private static UserDto ToDto(User user) => new(
        user.Id,
        user.Email,
        user.FirstName ?? string.Empty,
        user.LastName ?? string.Empty,
        user.CreatedAt);

    private static void ValidateRegistration(RegisterRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.FirstName))
            errors["firstName"] = ["First name is required."];
        if (string.IsNullOrWhiteSpace(request.LastName))
            errors["lastName"] = ["Last name is required."];
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors["email"] = ["Email is required."];
        }
        else
        {
            try
            {
                var parsedEmail = new System.Net.Mail.MailAddress(request.Email.Trim());
                if (!parsedEmail.Address.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase))
                    errors["email"] = ["Email must be valid."];
            }
            catch (FormatException)
            {
                errors["email"] = ["Email must be valid."];
            }
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["Password is required."];
        }
        else
        {
            var passwordErrors = new List<string>();
            if (request.Password.Length < 8 || request.Password.Length > 64)
                passwordErrors.Add("Password must be 8–64 characters long.");
            if (!request.Password.Any(char.IsLetter))
                passwordErrors.Add("Password must contain at least one letter.");
            if (!request.Password.Any(char.IsDigit))
                passwordErrors.Add("Password must contain at least one number.");
            if (!request.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                passwordErrors.Add("Password must contain at least one special character.");
            if (passwordErrors.Count > 0)
                errors["password"] = passwordErrors.ToArray();
        }

        if (errors.Count > 0)
            throw new InvalidRequestException(errors);
    }
}
