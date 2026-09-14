namespace Api.Models.Entities;

public class StripeEvent
{
    public string Id { get; set; } = string.Empty; 
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}