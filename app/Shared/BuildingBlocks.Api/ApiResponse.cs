using System.Text.Json.Serialization;

namespace BuildingBlocks.Api;

// O envelope único de toda resposta com corpo. "data" e "error" são sempre
// serializados (um deles nulo); "meta" só aparece quando há algo a dizer.
public sealed record ApiResponse<T>(
    T? Data,
    ApiError? Error,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] ApiMeta? Meta = null);

public sealed record ApiError(
    string Code,
    string Message,
    string Type,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ApiErrorDetail>? Details,
    string? TraceId);

public sealed record ApiErrorDetail(string Field, string Message);

public sealed record ApiMeta(PaginationMeta? Pagination);

public sealed record PaginationMeta(DateTime? NextCursor, bool HasMore);

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(T data, ApiMeta? meta = null) => new(data, null, meta);

    public static ApiResponse<object> Fail(ApiError error) => new(null, error);
}
