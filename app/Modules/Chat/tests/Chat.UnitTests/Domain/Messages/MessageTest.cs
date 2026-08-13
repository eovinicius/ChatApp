using Chat.Domain.Messages;

using FluentAssertions;

namespace Chat.UnitTests.Domain.Messages;

public class MessageTest
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    private static Message TextMessage(Guid senderId, DateTime? sentAt = null)
        => Message.Create(Guid.NewGuid(), senderId, MessageContent.CreateText("olá").Value, sentAt ?? Now).Value;

    [Fact]
    public void Deveria_criar_mensagem_de_texto()
    {
        // Arrange
        var senderId = Guid.NewGuid();

        // Act
        var message = TextMessage(senderId);

        // Assert
        message.SenderId.Should().Be(senderId);
        message.Content.Value.Should().Be("olá");
        message.IsTextMessage.Should().BeTrue();
        message.IsDeleted.Should().BeFalse();
        message.IsEdited.Should().BeFalse();
    }

    [Fact]
    public void Nao_deveria_criar_conteudo_de_texto_vazio()
    {
        // Act
        var result = MessageContent.CreateText("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.EmptyContent");
    }

    [Fact]
    public void Nao_deveria_criar_midia_sem_chave_de_storage()
    {
        // Act
        var result = MessageContent.CreateMedia(ContentType.Image, "https://cdn/x.png", "  ", "x.png", 10);

        // Assert — sem a key não há como apagar o objeto depois.
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.MissingStorageKey");
    }

    [Fact]
    public void Deveria_criar_midia_com_chave_de_storage()
    {
        // Act
        var result = MessageContent.CreateMedia(ContentType.Image, "https://cdn/x.png", "messages/x.png", "x.png", 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StorageKey.Should().Be("messages/x.png");
        result.Value.Type.Should().Be(ContentType.Image);
    }

    [Fact]
    public void Nao_deveria_editar_mensagem_de_outro_autor()
    {
        // Arrange
        var message = TextMessage(Guid.NewGuid());

        // Act
        var result = message.Edit(Guid.NewGuid(), "novo", Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.Unauthorized");
    }

    [Fact]
    public void Nao_deveria_editar_apos_a_janela_de_uma_hora()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var message = TextMessage(senderId);

        // Act
        var result = message.Edit(senderId, "novo", Now.AddHours(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.EditWindowExpired");
    }

    [Fact]
    public void Nao_deveria_editar_mensagem_de_midia()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var content = MessageContent.CreateMedia(ContentType.Image, "https://cdn/x.png", "messages/x.png", null, null).Value;
        var message = Message.Create(Guid.NewGuid(), senderId, content, Now).Value;

        // Act
        var result = message.Edit(senderId, "novo", Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.NotTextMessage");
    }

    [Fact]
    public void Deveria_editar_dentro_da_janela()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var message = TextMessage(senderId);

        // Act
        var result = message.Edit(senderId, "novo texto", Now.AddMinutes(30));

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Content.Value.Should().Be("novo texto");
        message.IsEdited.Should().BeTrue();
    }

    [Fact]
    public void Deveria_apagar_de_forma_logica()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var message = TextMessage(senderId);

        // Act
        var result = message.Delete(senderId, Now.AddHours(1));

        // Assert — soft delete: a linha continua, sustentando a contagem de não-lidas.
        result.IsSuccess.Should().BeTrue();
        message.IsDeleted.Should().BeTrue();
        message.DeletedAt.Should().Be(Now.AddHours(1));
    }

    [Fact]
    public void Nao_deveria_apagar_duas_vezes()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var message = TextMessage(senderId);
        message.Delete(senderId, Now);

        // Act
        var result = message.Delete(senderId, Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.AlreadyDeleted");
    }

    [Fact]
    public void Nao_deveria_apagar_apos_a_janela_de_24_horas()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var message = TextMessage(senderId);

        // Act
        var result = message.Delete(senderId, Now.AddHours(25));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.DeleteWindowExpired");
    }
}
