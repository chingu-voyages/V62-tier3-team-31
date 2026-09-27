using Ecommerce.Application.Common.Exceptions;
using System.Text.Json;

namespace Ecommerce.Middlewares.Api;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DuplicateEmailException)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, "Email is already registered", errors: null);
        }
        catch (InvalidRequestException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, ex.Message, ex.Errors);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(context, StatusCodes.Status401Unauthorized, ex.Message, errors: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred", errors: null);
        }
    }

    private static async Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        Dictionary<string, string[]>? errors)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        object body = errors is null
            ? new { title, status }
            : new { title, status, errors };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

