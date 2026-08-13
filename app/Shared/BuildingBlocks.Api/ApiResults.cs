using Microsoft.AspNetCore.Http;

using SharedKernel;

namespace BuildingBlocks.Api;

public static class ApiResults
{
    public static IResult Problem(Error error) =>
        Results.Problem(
            statusCode: ToStatusCode(error.Type),
            title: error.Code,
            detail: error.Name);

    // Sucesso: 204 No Content.
    public static IResult ToHttpResult(this Result result) =>
        result.IsFailure ? Problem(result.Error) : Results.NoContent();

    // Sucesso: 200 OK com o valor.
    public static IResult ToHttpResult<TValue>(this Result<TValue> result) =>
        result.IsFailure ? Problem(result.Error) : Results.Ok(result.Value);

    // Sucesso com contrato customizado (201 Created, envelope { token }, etc.).
    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess) =>
        result.IsFailure ? Problem(result.Error) : onSuccess();

    public static IResult ToHttpResult<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        result.IsFailure ? Problem(result.Error) : onSuccess(result.Value);

    private static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };
}
