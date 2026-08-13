namespace SharedKernel;

public record Error(string Code, string Name, ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.Validation);
}

// Agrega falhas por campo num único Error, para que a validação de request
// (DataAnnotations) e a de domínio saiam no mesmo formato. Cada item usa
// Code = nome do campo e Name = mensagem.
public sealed record ValidationError(IReadOnlyList<Error> Errors)
    : Error("Validation.Failed", "Um ou mais campos são inválidos.", ErrorType.Validation);
