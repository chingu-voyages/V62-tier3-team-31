namespace Ecommerce.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Title,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    CategoryDto Category);

public sealed record CategoryDto(Guid Id, string Name, string Slug);

public sealed record PagedResponseDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Limit,
    int Total,
    int TotalPages);

public sealed record CategoriesResponseDto(IReadOnlyList<CategoryDto> Categories);