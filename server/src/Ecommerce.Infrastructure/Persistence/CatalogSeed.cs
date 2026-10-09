using System;

namespace Ecommerce.Infrastructure.Persistence;

// Fixed ids and dates, so the migration stays the same every time it is generated.
// All categories and the first four products use the same ids as the frontend mock data.
public static class CatalogSeed
{
    private static readonly DateTime Start = new(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Guid Electronics = Guid.Parse("2b8d4a1c-5b85-4a27-934f-4f6c76f2e501");
    private static readonly Guid Books = Guid.Parse("8f0a7df6-22c2-4900-8e47-b3d2d6be8bc6");
    private static readonly Guid Kitchen = Guid.Parse("d4c2d79d-b3c3-4b26-85e8-dad8f89dbcd1");
    private static readonly Guid Stationery = Guid.Parse("a0e6ccf6-555b-48c8-b4e6-26826a464e26");
    private static readonly Guid Toys = Guid.Parse("ec0d92a8-8ec8-4d21-8d2e-29c2f9c38430");

    public static readonly object[] Categories =
    [
        MakeCategory(Electronics, "Electronics", "electronics"),
        MakeCategory(Books, "Books", "books"),
        MakeCategory(Kitchen, "Kitchen", "kitchen"),
        MakeCategory(Stationery, "Stationery", "stationery"),
        MakeCategory(Toys, "Toys", "toys"),
    ];

    public static readonly object[] Products =
    [
        MakeProduct("1f623603-229d-40ac-a52f-5c040efb174a", Electronics, "Nexora One X", "256GB · 6.7\" AMOLED · 5G", 999m, 8, 1),
        MakeProduct("bc224020-0bf0-4eb4-88b2-1fe2b0408a86", Electronics, "AeroBook 14", "14\" 2.8K · 16GB RAM · 512GB SSD", 899m, 6, 2),
        MakeProduct("cf578350-5640-4d0e-b676-1fc32b6c0c83", Electronics, "Pulse ANC Pro", "Wireless · Noise cancelling · 50h", 249m, 3, 3),
        MakeProduct("1303f95d-a3b1-4697-8721-72c624d7e9e9", Electronics, "Volt G15", "15.6\" QHD · RTX-class graphics · 16GB RAM", 1299m, 5, 4),
        MakeProduct("5d6ab7cc-d3d1-434b-bd74-8f388ff04cf7", Books, "Systems Programming in C", "A hands-on guide to memory, processes and the kernel.", 34.99m, 12, 5),
        MakeProduct("245ad70d-64e3-4721-b73a-d6adc324f1bb", Kitchen, "Cast Iron Skillet 10in", "Pre-seasoned skillet for the stove and the oven.", 34.99m, 15, 6),
        MakeProduct("ff7715c2-3e0a-4229-8b8d-a4de405d3ca8", Stationery, "Dot Grid Notebook A5", "160 pages of 100gsm paper with a lay-flat spine.", 12.99m, 40, 7),
        MakeProduct("eb52a79a-de3b-48e3-bced-93c9335e3949", Toys, "Wooden Building Blocks Set", "100 smooth beechwood blocks in a cotton bag.", 24.99m, 25, 8),
    ];

    private static object MakeCategory(Guid id, string name, string slug) => new
    {
        Id = id,
        Name = name,
        Slug = slug,
        CreatedAt = Start,
        UpdatedAt = Start,
    };

    private static object MakeProduct(string id, Guid categoryId, string title, string description, decimal price, int stock, int minutes) => new
    {
        Id = Guid.Parse(id),
        CategoryId = categoryId,
        Title = title,
        Description = description,
        Price = price,
        StockQuantity = stock,
        ImageUrl = (string?)null,
        IsActive = true,
        CreatedAt = Start.AddMinutes(minutes),
        UpdatedAt = Start.AddMinutes(minutes),
    };
}
