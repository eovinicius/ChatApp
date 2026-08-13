using FluentAssertions;

using SharedKernel;

namespace Chat.UnitTests.Domain.Abstractions;

public class ErrorTest
{
    [Fact]
    public void Error_None_Deve_Ser_Vazio()
    {
        Error.None.Code.Should().BeEmpty();
        Error.None.Name.Should().BeEmpty();
        Error.None.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Error_NullValue_Deve_Ser_Validation()
    {
        Error.NullValue.Code.Should().Be("Error.NullValue");
        Error.NullValue.Name.Should().Be("Null value was provided");
        Error.NullValue.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Error_Deve_Ter_Igualdade_Estrutural()
    {
        var primeiro = new Error("Test.Error", "Erro", ErrorType.NotFound);
        var segundo = new Error("Test.Error", "Erro", ErrorType.NotFound);

        primeiro.Should().Be(segundo);
    }

    [Fact]
    public void Error_Com_Type_Diferente_Nao_Deve_Ser_Igual()
    {
        var notFound = new Error("Test.Error", "Erro", ErrorType.NotFound);
        var conflict = new Error("Test.Error", "Erro", ErrorType.Conflict);

        notFound.Should().NotBe(conflict);
    }
}
