namespace Ecommerce.Core.Entities;

public enum OrderStatus
{
    Pending,
    Paid,
    Failed,
    Cancelled,
    Refunded,
    PartiallyRefunded,
    Disputed
}

public enum FulfillmentStatus
{
    Unfulfilled,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}