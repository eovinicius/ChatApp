using System.Diagnostics;

using BuildingBlocks.Pagination;

using Microsoft.AspNetCore.Http;

using SharedKernel;

namespace BuildingBlocks.Api;

// Único ponto de construção de resposta HTTP da API. Não existe sobrecarga que
// aceite um IResult pronto: é isso que impede um endpoint novo de devolver um
// formato próprio e faz o padrão valer por compilação, não por disciplina.
public static class ApiResults
{
    public static IResult Problem(Error error) =>
        Problem(error, ToStatusCode(error.Type));

    // Status explícito, para erros que o próprio pipeline HTTP produz (405, 415,
    // 429) e não têm um ErrorType de domínio correspondente.
    public static IResult Problem(Error error, int statusCode) =>
        Results.Json(ApiResponse.Fail(ToApiError(error)), statusCode: statusCode);

    // Comando sem retorno: 204 sem corpo.
    public static IResult ToHttpResult(this Result result) =>
        result.IsFailure ? Problem(result.Error) : Results.NoContent();

    // 200 com o valor dentro de "data".
    public static IResult ToHttpResult<TValue>(this Result<TValue> result) =>
        result.IsFailure
            ? Problem(result.Error)
            : Results.Ok(ApiResponse.Ok(result.Value));

    // 200 quando o corpo público difere do retorno do handler (ex.: { token }).
    public static IResult ToHttpResult<TValue>(this Result<TValue> result, Func<TValue, object> map) =>
        result.IsFailure
            ? Problem(result.Error)
            : Results.Ok(ApiResponse.Ok(map(result.Value)));

    // 201 + Location, com o mesmo envelope no corpo.
    public static IResult ToCreatedResult<TValue>(
        this Result<TValue> result,
        Func<TValue, string> location,
        Func<TValue, object> map) =>
        result.IsFailure
            ? Problem(result.Error)
            : Results.Created(location(result.Value), ApiResponse.Ok(map(result.Value)));

    // 201 sem Location, para recursos que não têm rota GET própria.
    public static IResult ToCreatedResult<TValue>(this Result<TValue> result, Func<TValue, object> map) =>
        result.IsFailure
            ? Problem(result.Error)
            : Results.Json(ApiResponse.Ok(map(result.Value)), statusCode: StatusCodes.Status201Created);

    // Lista paginada: os itens em "data", o cursor em "meta.pagination".
    public static IResult ToPagedResult<TItem>(this Result<Page<TItem>> result)
    {
        if (result.IsFailure)
            return Problem(result.Error);

        var page = result.Value;

        return Results.Ok(ApiResponse.Ok(
            page.Items,
            new ApiMeta(new PaginationMeta(page.NextCursor, page.HasMore))));
    }

    private static ApiError ToApiError(Error error) => new(
        error.Code,
        error.Name,
        error.Type.ToString(),
        error is ValidationError validation
            ? validation.Errors.Select(item => new ApiErrorDetail(item.Code, item.Name)).ToList()
            : null,
        Activity.Current?.Id);

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
