using System.Globalization;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class ProductsController : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultLimit = 20;
    private const int MaximumLimit = 50;
    private const int MaximumStockQuantity = 10;
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("products")]
    public async Task<ActionResult<PagedResponseDto<ProductDto>>> GetProducts(
        [FromQuery] string? page,
        [FromQuery] string? limit,
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] string? sort)
    {
        var errors = new Dictionary<string, string[]>();
        var parsedPage = ParseInteger(page, "page", DefaultPage, value => value >= 1, "Page must be 1 or greater.", errors);
        var parsedLimit = ParseInteger(limit, "limit", DefaultLimit, value => value is >= 1 and <= MaximumLimit, "Limit must be between 1 and 50.", errors);

        var normalizedSearch = search?.Trim();
        if (normalizedSearch?.Length > 100)
            errors["search"] = ["Search must be 100 characters or fewer."];

        var normalizedSort = sort ?? "newest";
        if (normalizedSort is not ("newest" or "price_asc" or "price_desc" or "title_asc"))
            errors["sort"] = ["Sort must be one of newest, price_asc, price_desc, or title_asc."];

        if (errors.Count > 0)
            throw new InvalidRequestException(errors);

        var productsQuery = _context.Products
            .AsNoTracking()
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
            productsQuery = productsQuery.Where(product => product.Category.Slug == category);

        if (!string.IsNullOrEmpty(normalizedSearch))
        {
            var escapedSearch = normalizedSearch.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            productsQuery = productsQuery.Where(product => EF.Functions.ILike(product.Title, $"%{escapedSearch}%"));
        }

        productsQuery = (normalizedSort switch
        {
            "price_asc" => productsQuery.OrderBy(product => product.Price),
            "price_desc" => productsQuery.OrderByDescending(product => product.Price),
            "title_asc" => productsQuery.OrderBy(product => product.Title),
            _ => productsQuery.OrderByDescending(product => product.CreatedAt)
        }).ThenBy(product => product.Id);

        var total = await productsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(total / (double)parsedLimit);
        var items = totalPages == 0 || parsedPage > totalPages
            ? Array.Empty<ProductDto>()
            : (await productsQuery
                .Include(product => product.Category)
                .Skip((int)((long)(parsedPage - 1) * parsedLimit))
                .Take(parsedLimit)
                .ToListAsync())
            .Select(ToDto)
            .ToArray();

        return Ok(new PagedResponseDto<ProductDto>(items, parsedPage, parsedLimit, total, totalPages));
    }

    [HttpGet("products/{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(string id)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            throw new InvalidRequestException(new Dictionary<string, string[]>
            {
                ["id"] = ["ID must be a valid UUID."]
            });
        }

        var product = await _context.Products
            .AsNoTracking()
            .Include(candidate => candidate.Category)
            .FirstOrDefaultAsync(candidate => candidate.Id == productId && candidate.IsActive);

        return product is null
            ? throw new ProductNotFoundException()
            : Ok(ToDto(product));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<CategoriesResponseDto>> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryDto(category.Id, category.Name, category.Slug))
            .ToListAsync();

        return Ok(new CategoriesResponseDto(categories));
    }

    private static int ParseInteger(
        string? rawValue,
        string field,
        int defaultValue,
        Func<int, bool> isValid,
        string errorMessage,
        Dictionary<string, string[]> errors)
    {
        if (rawValue is null)
            return defaultValue;

        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || !isValid(value))
            errors[field] = [errorMessage];

        return value;
    }

    private static ProductDto ToDto(Ecommerce.Core.Entities.Product product) => new(
        product.Id,
        product.Title,
        product.Description,
        decimal.Round(product.Price, 2, MidpointRounding.AwayFromZero),
        Math.Clamp(product.StockQuantity, 0, MaximumStockQuantity),
        product.ImageUrl,
        new CategoryDto(product.Category.Id, product.Category.Name, product.Category.Slug));
}