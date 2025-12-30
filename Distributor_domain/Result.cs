namespace Distributor_domain;

public readonly record struct Result<T>(
bool IsSuccess,
T? Value,
Error? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(Error error) => new(false, default, error);
}

public sealed record Error(string Code, string Message);
