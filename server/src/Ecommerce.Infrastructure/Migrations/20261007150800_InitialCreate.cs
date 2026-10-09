using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ecommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:fulfillment_status", "unfulfilled,processing,shipped,delivered,cancelled")
                .Annotation("Npgsql:Enum:order_status", "pending,paid,failed,cancelled,refunded,partially_refunded,disputed");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StripeEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "order_status", nullable: false),
                    FulfillmentStatus = table.Column<int>(type: "fulfillment_status", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ShippingName = table.Column<string>(type: "text", nullable: false),
                    ShippingPhone = table.Column<string>(type: "text", nullable: true),
                    ShippingAddressLine1 = table.Column<string>(type: "text", nullable: false),
                    ShippingAddressLine2 = table.Column<string>(type: "text", nullable: true),
                    ShippingCity = table.Column<string>(type: "text", nullable: false),
                    ShippingState = table.Column<string>(type: "text", nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "text", nullable: false),
                    ShippingCountry = table.Column<string>(type: "text", nullable: false),
                    StripeSessionId = table.Column<string>(type: "text", nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "text", nullable: true),
                    RefundedAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTitle = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Name", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2b8d4a1c-5b85-4a27-934f-4f6c76f2e501"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Electronics", "electronics", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("8f0a7df6-22c2-4900-8e47-b3d2d6be8bc6"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Books", "books", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0e6ccf6-555b-48c8-b4e6-26826a464e26"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stationery", "stationery", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d4c2d79d-b3c3-4b26-85e8-dad8f89dbcd1"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kitchen", "kitchen", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ec0d92a8-8ec8-4d21-8d2e-29c2f9c38430"), new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Toys", "toys", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsActive", "Price", "StockQuantity", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1303f95d-a3b1-4697-8721-72c624d7e9e9"), new Guid("2b8d4a1c-5b85-4a27-934f-4f6c76f2e501"), new DateTime(2026, 10, 1, 0, 4, 0, 0, DateTimeKind.Utc), "15.6\" QHD · RTX-class graphics · 16GB RAM", null, true, 1299m, 5, "Volt G15", new DateTime(2026, 10, 1, 0, 4, 0, 0, DateTimeKind.Utc) },
                    { new Guid("1f623603-229d-40ac-a52f-5c040efb174a"), new Guid("2b8d4a1c-5b85-4a27-934f-4f6c76f2e501"), new DateTime(2026, 10, 1, 0, 1, 0, 0, DateTimeKind.Utc), "256GB · 6.7\" AMOLED · 5G", null, true, 999m, 8, "Nexora One X", new DateTime(2026, 10, 1, 0, 1, 0, 0, DateTimeKind.Utc) },
                    { new Guid("245ad70d-64e3-4721-b73a-d6adc324f1bb"), new Guid("d4c2d79d-b3c3-4b26-85e8-dad8f89dbcd1"), new DateTime(2026, 10, 1, 0, 6, 0, 0, DateTimeKind.Utc), "Pre-seasoned skillet for the stove and the oven.", null, true, 34.99m, 15, "Cast Iron Skillet 10in", new DateTime(2026, 10, 1, 0, 6, 0, 0, DateTimeKind.Utc) },
                    { new Guid("5d6ab7cc-d3d1-434b-bd74-8f388ff04cf7"), new Guid("8f0a7df6-22c2-4900-8e47-b3d2d6be8bc6"), new DateTime(2026, 10, 1, 0, 5, 0, 0, DateTimeKind.Utc), "A hands-on guide to memory, processes and the kernel.", null, true, 34.99m, 12, "Systems Programming in C", new DateTime(2026, 10, 1, 0, 5, 0, 0, DateTimeKind.Utc) },
                    { new Guid("bc224020-0bf0-4eb4-88b2-1fe2b0408a86"), new Guid("2b8d4a1c-5b85-4a27-934f-4f6c76f2e501"), new DateTime(2026, 10, 1, 0, 2, 0, 0, DateTimeKind.Utc), "14\" 2.8K · 16GB RAM · 512GB SSD", null, true, 899m, 6, "AeroBook 14", new DateTime(2026, 10, 1, 0, 2, 0, 0, DateTimeKind.Utc) },
                    { new Guid("cf578350-5640-4d0e-b676-1fc32b6c0c83"), new Guid("2b8d4a1c-5b85-4a27-934f-4f6c76f2e501"), new DateTime(2026, 10, 1, 0, 3, 0, 0, DateTimeKind.Utc), "Wireless · Noise cancelling · 50h", null, true, 249m, 3, "Pulse ANC Pro", new DateTime(2026, 10, 1, 0, 3, 0, 0, DateTimeKind.Utc) },
                    { new Guid("eb52a79a-de3b-48e3-bced-93c9335e3949"), new Guid("ec0d92a8-8ec8-4d21-8d2e-29c2f9c38430"), new DateTime(2026, 10, 1, 0, 8, 0, 0, DateTimeKind.Utc), "100 smooth beechwood blocks in a cotton bag.", null, true, 24.99m, 25, "Wooden Building Blocks Set", new DateTime(2026, 10, 1, 0, 8, 0, 0, DateTimeKind.Utc) },
                    { new Guid("ff7715c2-3e0a-4229-8b8d-a4de405d3ca8"), new Guid("a0e6ccf6-555b-48c8-b4e6-26826a464e26"), new DateTime(2026, 10, 1, 0, 7, 0, 0, DateTimeKind.Utc), "160 pages of 100gsm paper with a lay-flat spine.", null, true, 12.99m, 40, "Dot Grid Notebook A5", new DateTime(2026, 10, 1, 0, 7, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId_ProductId",
                table: "CartItems",
                columns: new[] { "CartId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId",
                table: "CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_SessionId",
                table: "Carts",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "StripeEvents");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
