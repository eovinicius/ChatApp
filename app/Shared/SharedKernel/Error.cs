namespace SharedKernel;

public sealed record Error(string Code, string Name, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Validation);
}
