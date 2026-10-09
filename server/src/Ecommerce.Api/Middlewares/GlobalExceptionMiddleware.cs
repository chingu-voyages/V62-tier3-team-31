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
        catch (CartNotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, ex.Message, errors: null);
        }
        catch (ProductNotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, ex.Message, errors: null);
        }
        catch (CartConflictException ex)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, ex.Message, errors: null);
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
            var detail = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? ex.ToString()
                : null;
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "Something went wrong", errors: null, detail);
        }
    }

    private static async Task WriteAsync(
        HttpContext context,
        int status,
        string title,
        Dictionary<string, string[]>? errors,
        string? detail = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        object body = errors is not null
            ? new { title, status, errors }
            : detail is not null
                ? new { title, status, detail }
                : new { title, status };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}

