namespace Ecommerce.Core.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } 
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public User? User { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}