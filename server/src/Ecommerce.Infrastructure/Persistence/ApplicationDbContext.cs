using Api.Models.Entities;
using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>(); // Nuevo
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<StripeEvent> StripeEvents => Set<StripeEvent>(); // Nuevo
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>(); // Nuevo

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.HasPostgresEnum<OrderStatus>();
        modelBuilder.HasPostgresEnum<FulfillmentStatus>();
        
        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Status)
                  .HasColumnType("order_status");

            entity.Property(e => e.FulfillmentStatus)
                  .HasColumnType("fulfillment_status");
        });

        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.SessionId)
            .IsUnique();

        modelBuilder.Entity<StripeEvent>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
    }
}