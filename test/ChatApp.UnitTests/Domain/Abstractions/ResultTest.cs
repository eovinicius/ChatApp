using ChatApp.Domain.Abstractions;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.Abstractions;

public class ResultTest
{
    [Fact]
    public void Result_Success_Deve_Ter_IsSuccess_True()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Result_Failure_Deve_Ter_IsFailure_True()
    {
        var error = new Error("Test.Error", "Erro de teste");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Result_Generic_Success_Deve_Retornar_Valor()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_Generic_Failure_Value_Deve_Lancar_Excecao()
    {
        var result = Result.Failure<int>(new Error("Test.Error", "Erro"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Result_Create_Com_Valor_Nao_Nulo_Deve_Ser_Sucesso()
    {
        var result = Result.Create("valor");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("valor");
    }

    [Fact]
    public void Result_Create_Com_Null_Deve_Ser_Falha()
    {
        var result = Result.Create<string>(null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void Result_Construtor_Invalido_Sucesso_Com_Erro_Deve_Lancar_Excecao()
    {
        var act = () => new Result(true, new Error("X", "Y"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Result_Construtor_Invalido_Falha_Com_Error_None_Deve_Lancar_Excecao()
    {
        var act = () => new Result(false, Error.None);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Result_Implicit_Conversion_De_Valor_Nao_Nulo_Deve_Ser_Sucesso()
    {
        Result<string> result = "valor-convertido";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("valor-convertido");
    }
}
