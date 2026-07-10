using Microsoft.AspNetCore.Http;

using SharedKernel;

namespace Chat.Presentation;

internal static class ApiResults
{
    public static IResult Problem(Error error) =>
        Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: error.Code,
            detail: error.Name);
}
