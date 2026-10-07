namespace Ecommerce.Application.Common.Exceptions;

public sealed class PaymentGatewayException : Exception
{
    public PaymentGatewayException(string message, Exception? inner = null) : base(message, inner) { }
}
