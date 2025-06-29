using ChatApp.Domain.Entities.ChatRooms;

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
        var messageContent = "Hello World";

        // Act
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, messageContent);

        // Assert
        chatMessage.Should().NotBeNull();
        chatMessage.Id.Should().NotBe(Guid.Empty);
        chatMessage.ChatRoomId.Should().Be(chatRoomId);
        chatMessage.SenderId.Should().Be(senderId);
        chatMessage.Content.Should().Be(messageContent);
        chatMessage.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        chatMessage.IsEdited.Should().BeFalse();
    }

    [Fact]
    public void Deveria_editar_mensagem_de_chat()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var messageContent = "Hello World";
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, messageContent);

        // Act
        var newContent = "Hello Universe";
        chatMessage.Edit(newContent, senderId, DateTime.UtcNow);

        // Assert
        chatMessage.Content.Should().Be(newContent);
        chatMessage.IsEdited.Should().BeTrue();
    }

    [Fact]
    public void Nao_deveria_editar_mensagem_de_chat_por_outro_usuario()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var messageContent = "Hello World";
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, messageContent);
        var otherUserId = Guid.NewGuid();

        // Act
        var result = chatMessage.Edit("New Content", otherUserId, DateTime.UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        chatMessage.Content.Should().Be(messageContent);
        chatMessage.IsEdited.Should().BeFalse();
    }

    [Fact]
    public void Deveria_poder_deletar_mensagem_de_chat()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();
        var messageContent = "Hello World";
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, messageContent);

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
        var messageContent = "Hello World";
        var chatMessage = ChatMessage.Create(chatRoomId, senderId, messageContent);
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
