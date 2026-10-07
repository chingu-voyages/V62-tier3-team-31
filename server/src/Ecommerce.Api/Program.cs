using System.Text;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Constants.Api;
using Ecommerce.Core.Entities;
using Ecommerce.Infrastructure.Services;
using Ecommerce.Middlewares.Api;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var accessSecret = jwtSettings["AccessTokenSecret"];
var refreshSecret = jwtSettings["RefreshTokenSecret"];
if (string.IsNullOrWhiteSpace(accessSecret) || string.IsNullOrWhiteSpace(refreshSecret))
    throw new InvalidOperationException(
        "JWT secrets are not configured. Set JwtSettings:AccessTokenSecret and JwtSettings:RefreshTokenSecret.");
var issuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured.");
var audience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");
// Postgres enums must be registered with the driver as well as declared in the model,
// otherwise Npgsql refuses to read or write order_status and fulfillment_status.
var dataSource = new Npgsql.NpgsqlDataSourceBuilder(connectionString)
    .MapEnum<Ecommerce.Core.Entities.OrderStatus>("order_status")
    .MapEnum<Ecommerce.Core.Entities.FulfillmentStatus>("fulfillment_status")
    .Build();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource, npgsql =>
    {
        // EF needs the mapping as well as the driver. Newer Npgsql no longer reads it from the data source.
        npgsql.MapEnum<Ecommerce.Core.Entities.OrderStatus>("order_status");
        npgsql.MapEnum<Ecommerce.Core.Entities.FulfillmentStatus>("fulfillment_status");
    }));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origin = builder.Configuration["Cors:FrontendOrigin"]
            ?? throw new InvalidOperationException("Cors:FrontendOrigin is not configured.");

        policy.WithOrigins(origin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;    
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(accessSecret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { title = "Not logged in", status = StatusCodes.Status401Unauthorized }),
                context.HttpContext.RequestAborted);
        },
        OnMessageReceived = context =>
        {

            if (context.Request.Cookies.TryGetValue(AuthCookieNames.AccessToken, out var token))
            {
                context.Token = token;
            }
            
            else if (string.IsNullOrEmpty(context.Token))
            {
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader["Bearer ".Length..].Trim();
                }
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseForwardedHeaders();
app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
