using Chat.Domain.Messages;

using FluentAssertions;

namespace Chat.UnitTests.Domain.Messages;

public class ContentTypeTest
{
    [Theory]
    [InlineData("text")]
    [InlineData("image")]
    [InlineData("audio")]
    [InlineData("video")]
    [InlineData("file")]
    public void Deveria_converter_tipos_suportados(string value)
    {
        // Act
        var result = ContentType.From(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("TEXT")]
    [InlineData("  Image  ")]
    public void Deveria_ignorar_caixa_e_espacos(string value)
    {
        // Act
        var result = ContentType.From(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("sticker")]
    [InlineData("")]
    [InlineData(null)]
    public void Deveria_falhar_em_tipo_nao_suportado(string? value)
    {
        // Act
        var result = ContentType.From(value);

        // Assert — falha esperada vira Result, não exceção.
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidContentType");
    }

    [Fact]
    public void Deveria_identificar_midia()
    {
        // Assert
        ContentType.Text.IsMedia.Should().BeFalse();
        ContentType.Image.IsMedia.Should().BeTrue();
        ContentType.Audio.IsMedia.Should().BeTrue();
    }
}
