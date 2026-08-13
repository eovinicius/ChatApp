using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

using SharedKernel;

namespace BuildingBlocks.Api;

// Valida (via DataAnnotations) o argumento do tipo T recebido pelo endpoint,
// preservando o comportamento de auto-validação que os Controllers [ApiController] tinham.
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
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
                // Uma falha por (campo, mensagem) — o mesmo Error que o domínio
                // usa, para que os 400 tenham todos a mesma forma.
                var errors = results
                    .SelectMany(
                        result => result.MemberNames.DefaultIfEmpty(string.Empty),
                        (result, member) => new Error(
                            member,
                            result.ErrorMessage ?? "Valor inválido",
                            ErrorType.Validation))
                    .ToList();

                return ApiResults.Problem(new ValidationError(errors));
            }
        }

        return await next(context);
    }
}
