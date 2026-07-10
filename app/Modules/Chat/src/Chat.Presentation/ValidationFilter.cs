using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace Chat.Presentation;

// Valida (via DataAnnotations) o argumento do tipo T recebido pelo endpoint,
// preservando o comportamento de auto-validação que os Controllers [ApiController] tinham.
internal sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is not null)
        {
            var results = new List<ValidationResult>();
            var validationContext = new ValidationContext(argument);

            if (!Validator.TryValidateObject(argument, validationContext, results, validateAllProperties: true))
            {
                var errors = results
                    .SelectMany(
                        result => result.MemberNames.DefaultIfEmpty(string.Empty),
                        (result, member) => (Member: member, result.ErrorMessage))
                    .GroupBy(entry => entry.Member)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(entry => entry.ErrorMessage ?? "Valor inválido").ToArray());

                return Results.ValidationProblem(errors);
            }
        }

        return await next(context);
    }
}
