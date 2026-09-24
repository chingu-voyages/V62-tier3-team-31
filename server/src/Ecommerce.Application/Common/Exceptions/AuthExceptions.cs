namespace Ecommerce.Application.Common.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("Email is already registered") { }
}

public sealed class InvalidRequestException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public InvalidRequestException(Dictionary<string, string[]> errors)
        : base("Validation failed")
    {
        Errors = errors;
    }
}
