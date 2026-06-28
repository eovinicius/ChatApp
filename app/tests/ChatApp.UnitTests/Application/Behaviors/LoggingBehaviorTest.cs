using ChatApp.Application.Abstractions.Behaviors;
using ChatApp.Domain.Abstractions;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Behaviors;

public record LoggingTestCommand : IRequest<Result>;

public class LoggingBehaviorTest
{
    private readonly ILogger<LoggingBehavior<LoggingTestCommand, Result>> _loggerMock =
        Substitute.For<ILogger<LoggingBehavior<LoggingTestCommand, Result>>>();

    [Fact]
    public async Task Handle_Deve_Retornar_Resultado_De_Sucesso()
    {
        var behavior = new LoggingBehavior<LoggingTestCommand, Result>(_loggerMock);
        var command = new LoggingTestCommand();

        var result = await behavior.Handle(command, ct => Task.FromResult(Result.Success()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Resultado_De_Falha()
    {
        var behavior = new LoggingBehavior<LoggingTestCommand, Result>(_loggerMock);
        var command = new LoggingTestCommand();
        var error = new Error("Test.Fail", "Falhou");

        var result = await behavior.Handle(command, _ => Task.FromResult(Result.Failure(error)), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task Handle_Deve_Propagar_Excecao()
    {
        var behavior = new LoggingBehavior<LoggingTestCommand, Result>(_loggerMock);
        var command = new LoggingTestCommand();

        var act = () => behavior.Handle(command, ct => throw new InvalidOperationException("erro inesperado"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
