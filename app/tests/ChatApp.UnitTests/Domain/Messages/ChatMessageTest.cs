using ChatApp.Domain.Entities.Messages;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.Messages;

public class ChatMessageTest
{
    [Fact]
    public void Deveria_criar_mensagem_de_chat()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var contentType = ContentType.Text;
        var contentData = "hello world";
        var currentUtcTime = DateTime.UtcNow;

        // Act
        var result = ChatMessage.Create(chatRoomId, contentType, contentData, senderId, currentUtcTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var chatMessage = result.Value;
        chatMessage.Id.Should().NotBe(Guid.Empty);
        chatMessage.ChatRoomId.Should().Be(chatRoomId);
        chatMessage.SenderId.Should().Be(senderId);
        chatMessage.ContentType.Should().Be(ContentType.Text);
        chatMessage.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        chatMessage.IsEdited.Should().BeFalse();
    }

    [Fact]
    public void Nao_deveria_criar_mensagem_de_texto_com_conteudo_vazio()
    {
        var result = ChatMessage.Create(Guid.NewGuid(), ContentType.Text, "   ", Guid.NewGuid(), DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ChatMessage.EmptyContent");
    }

    [Fact]
    public void Deveria_criar_mensagem_de_imagem_sem_validar_conteudo_como_texto()
    {
        var result = ChatMessage.Create(Guid.NewGuid(), ContentType.Image, "s3-key/image.png", Guid.NewGuid(), DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Deveria_editar_mensagem_de_chat()
    {
        // Arrange
        var chatMessage = ChatMessage.Create(Guid.NewGuid(), ContentType.Text, "hello world", Guid.NewGuid(), DateTime.UtcNow).Value;

        // Act
        chatMessage.Edit("Hello Universe", DateTime.UtcNow);

        // Assert
        chatMessage.Content.Should().Be("Hello Universe");
        chatMessage.ContentType.Should().Be(ContentType.Text);
        chatMessage.IsEdited.Should().BeTrue();
    }

    [Fact]
    public void Deveria_poder_deletar_mensagem_de_chat()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var chatMessage = ChatMessage.Create(chatRoomId, ContentType.Text, "hello world", senderId, DateTime.UtcNow).Value;

        // Act
        var canDelete = chatMessage.CanBeDeletedBy(senderId, chatRoomId, DateTime.UtcNow);

        // Assert
        canDelete.Should().BeTrue();
    }

    [Fact]
    public void Nao_deveria_poder_deletar_mensagem_de_chat_por_outro_usuario()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var chatMessage = ChatMessage.Create(chatRoomId, ContentType.Text, "hello world", senderId, DateTime.UtcNow).Value;

        // Act
        var canDelete = chatMessage.CanBeDeletedBy(Guid.NewGuid(), chatRoomId, DateTime.UtcNow);

        // Assert
        canDelete.Should().BeFalse();
    }

    [Fact]
    public void Nao_deveria_poder_deletar_mensagem_de_chat_com_roomId_incorreto()
    {
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var chatMessage = ChatMessage.Create(chatRoomId, ContentType.Text, "hello world", senderId, DateTime.UtcNow).Value;

        var canDelete = chatMessage.CanBeDeletedBy(senderId, Guid.NewGuid(), DateTime.UtcNow);

        canDelete.Should().BeFalse();
    }

    [Fact]
    public void Nao_deveria_editar_mensagem_que_nao_e_de_texto()
    {
        var chatMessage = ChatMessage.Create(Guid.NewGuid(), ContentType.Image, "s3/img.png", Guid.NewGuid(), DateTime.UtcNow).Value;

        var result = chatMessage.Edit("novo conteudo", DateTime.UtcNow);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ChatMessage.NotTextMessage");
    }

    [Fact]
    public void Deveria_atualizar_EditedAt_na_segunda_edicao()
    {
        var sentAt = DateTime.UtcNow.AddMinutes(-10);
        var chatMessage = ChatMessage.Create(Guid.NewGuid(), ContentType.Text, "original", Guid.NewGuid(), sentAt).Value;

        var primeiraEdicao = DateTime.UtcNow.AddMinutes(-5);
        chatMessage.Edit("primeira edicao", primeiraEdicao);

        var segundaEdicao = DateTime.UtcNow;
        chatMessage.Edit("segunda edicao", segundaEdicao);

        chatMessage.Content.Should().Be("segunda edicao");
        chatMessage.EditedAt.Should().BeCloseTo(segundaEdicao, TimeSpan.FromMilliseconds(10));
        chatMessage.IsEdited.Should().BeTrue();
    }

    [Fact]
    public void Deveria_criar_mensagem_audio_com_conteudo_vazio_sem_erro()
    {
        var result = ChatMessage.Create(Guid.NewGuid(), ContentType.Audio, "", Guid.NewGuid(), DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
    }
}
