namespace Ecommerce.Application.Common.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("Email is already registered") { }
}

public sealed class InvalidRequestException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public InvalidRequestException(Dictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public sealed class CartNotFoundException : Exception
{
    public CartNotFoundException(string message) : base(message) { }
}

public sealed class CartConflictException : Exception
{
    public CartConflictException(string message) : base(message) { }
}

public sealed class ProductNotFoundException : Exception
{
    public ProductNotFoundException() : base("Product not found") { }
}
