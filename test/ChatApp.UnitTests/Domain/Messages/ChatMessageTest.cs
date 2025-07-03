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
        var contentType = "text";
        var contentData = "hello world";
        var currentUtcTime = DateTime.UtcNow;

        // Act
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, contentType, contentData, currentUtcTime);

        // Assert
        chatMessage.Should().NotBeNull();
        chatMessage.Id.Should().NotBe(Guid.Empty);
        chatMessage.ChatRoomId.Should().Be(chatRoomId);
        chatMessage.SenderId.Should().Be(senderId);
        chatMessage.Content.Type.Should().Be(ContentType.Text);
        chatMessage.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        chatMessage.IsEdited.Should().BeFalse();
    }

    [Fact]
    public void Deveria_editar_mensagem_de_chat()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var contentType = "text";
        var contentData = "hello world";
        var currentUtcTime = DateTime.UtcNow;
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, contentType, contentData, currentUtcTime);

        // Act
        var newContent = "Hello Universe";
        chatMessage.Edit(newContent, DateTime.UtcNow);

        // Assert
        chatMessage.Content.Type.Should().Be(ContentType.Text);
        chatMessage.IsEdited.Should().BeTrue();
    }

    [Fact]
    public void Deveria_poder_deletar_mensagem_de_chat()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var contentType = "text";
        var contentData = "hello world";
        var currentUtcTime = DateTime.UtcNow;
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, contentType, contentData, currentUtcTime);

        // Act
        var canDelete = chatMessage.CanBeDeletedBy(senderId, DateTime.UtcNow);

        // Assert
        canDelete.Should().BeTrue();
    }

    [Fact]
    public void Nao_deveria_poder_deletar_mensagem_de_chat_por_outro_usuario()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var contentType = "text";
        var contentData = "hello world";
        var currentUtcTime = DateTime.UtcNow;
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, contentType, contentData, currentUtcTime);
        var otherUserId = Guid.NewGuid();

        // Act
        var canDelete = chatMessage.CanBeDeletedBy(otherUserId, DateTime.UtcNow);

        // Assert
        canDelete.Should().BeFalse();
    }

    [Fact]
    public void Nao_deveria_permitir_deletar_mensagem_apos_limite_de_tempo()
    {
        //todo: implementar teste para verificar se a mensagem não pode ser deletada após o limite de tempo
    }
}
