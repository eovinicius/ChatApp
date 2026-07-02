using ChatApp.Application.Abstractions.Behaviors;
using ChatApp.Domain.Abstractions;

using FluentAssertions;

using MediatR;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Behaviors;

public record PerformanceTestCommand : IRequest<Result>;

public class PerformanceBehaviorTest
{
    [Fact]
    public async Task Deveria_retornar_resultado_para_requisicao_rapida()
    {
        var logger = NullLogger<PerformanceBehavior<PerformanceTestCommand, Result>>.Instance;
        var behavior = new PerformanceBehavior<PerformanceTestCommand, Result>(logger);
        var command = new PerformanceTestCommand();

        var result = await behavior.Handle(command, ct => Task.FromResult(Result.Success()), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Deveria_logar_warning_para_requisicao_lenta()
    {
        var loggerMock = Substitute.For<ILogger<PerformanceBehavior<PerformanceTestCommand, Result>>>();
        var behavior = new PerformanceBehavior<PerformanceTestCommand, Result>(loggerMock);
        var command = new PerformanceTestCommand();

        var result = await behavior.Handle(command, async ct =>
        {
            await Task.Delay(3500, ct);
            return Result.Success();
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        loggerMock.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
