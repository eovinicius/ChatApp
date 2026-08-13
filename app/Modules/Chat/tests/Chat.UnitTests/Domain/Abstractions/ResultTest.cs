using FluentAssertions;

using SharedKernel;

namespace Chat.UnitTests.Domain.Abstractions;

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
        var error = new Error("Test.Error", "Erro de teste", ErrorType.Failure);

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
        var result = Result.Failure<int>(new Error("Test.Error", "Erro", ErrorType.Failure));

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
        var act = () => new Result(true, new Error("X", "Y", ErrorType.Failure));

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

    [Fact]
    public void Result_Implicit_Conversion_De_Error_Deve_Ser_Falha()
    {
        var error = new Error("Test.NotFound", "Não encontrado", ErrorType.NotFound);

        Result result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Result_Generic_Implicit_Conversion_De_Error_Deve_Ser_Falha()
    {
        var error = new Error("Test.NotFound", "Não encontrado", ErrorType.NotFound);

        Result<int> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Result_Generic_Implicit_Conversion_De_Error_Deve_Funcionar_Em_Tipo_De_Retorno()
    {
        // Reproduz o cenário real: método declarado como Result<T> retornando um Error direto.
        static Result<string> ObterValor(bool encontrado) =>
            encontrado ? "encontrado" : new Error("Test.NotFound", "Não encontrado", ErrorType.NotFound);

        ObterValor(true).Value.Should().Be("encontrado");
        ObterValor(false).Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Result_ValueOrDefault_Deve_Retornar_Valor_Em_Sucesso()
    {
        var result = Result.Success("valor");

        result.ValueOrDefault.Should().Be("valor");
    }

    [Fact]
    public void Result_ValueOrDefault_Deve_Retornar_Default_Em_Falha()
    {
        Result<string> referencia = new Error("Test.Error", "Erro", ErrorType.Failure);
        Result<int> valor = new Error("Test.Error", "Erro", ErrorType.Failure);

        referencia.ValueOrDefault.Should().BeNull();
        valor.ValueOrDefault.Should().Be(0);
    }
}
