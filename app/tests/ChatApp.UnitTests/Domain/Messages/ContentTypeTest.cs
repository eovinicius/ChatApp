using ChatApp.Domain.Entities.Messages;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.Messages;

public class ContentTypeTest
{
    [Theory]
    [InlineData("text")]
    [InlineData("image")]
    [InlineData("audio")]
    [InlineData("video")]
    public void From_Deveria_Retornar_ContentType_Para_Valores_Validos(string value)
    {
        var result = ContentType.From(value);

        result.Value.Should().Be(value);
    }

    [Fact]
    public void From_Deveria_Ser_Case_Insensitive()
    {
        ContentType.From("TEXT").Should().Be(ContentType.Text);
        ContentType.From("Image").Should().Be(ContentType.Image);
    }

    [Fact]
    public void From_Deveria_Lancar_Excecao_Para_Tipo_Nao_Suportado()
    {
        var act = () => ContentType.From("document");

        act.Should().Throw<NotSupportedException>();
    }
}
