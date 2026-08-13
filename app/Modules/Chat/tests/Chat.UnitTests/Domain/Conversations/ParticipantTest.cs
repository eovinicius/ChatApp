using Chat.Domain.Conversations;

using FluentAssertions;

namespace Chat.UnitTests.Domain.Conversations;

// O cursor de leitura é a base dos recibos e da contagem de não-lidas.
public class ParticipantTest
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    private static (Conversation Conversation, Guid OwnerId, Guid MemberId) Group()
    {
        var ownerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var conversation = Conversation.CreateGroup("Time", ownerId, [memberId], Now).Value;

        return (conversation, ownerId, memberId);
    }

    [Fact]
    public void Deveria_gerar_id_proprio_para_o_participante()
    {
        // Arrange & Act
        var (conversation, ownerId, _) = Group();

        // Assert — o modelo antigo deixava todo Id em Guid.Empty.
        conversation.FindParticipant(ownerId)!.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Deveria_avancar_o_cursor_de_leitura()
    {
        // Arrange
        var (conversation, _, memberId) = Group();
        var messageId = Guid.NewGuid();

        // Act
        conversation.MarkRead(memberId, messageId, Now.AddMinutes(5));

        // Assert
        var participant = conversation.FindParticipant(memberId)!;
        participant.LastReadMessageId.Should().Be(messageId);
        participant.LastReadMessageSentAt.Should().Be(Now.AddMinutes(5));
    }

    [Fact]
    public void Nao_deveria_retroceder_o_cursor_de_leitura()
    {
        // Arrange
        var (conversation, _, memberId) = Group();
        var recente = Guid.NewGuid();
        conversation.MarkRead(memberId, recente, Now.AddMinutes(10));

        // Act — ack fora de ordem, vindo de outro dispositivo.
        conversation.MarkRead(memberId, Guid.NewGuid(), Now.AddMinutes(2));

        // Assert
        var participant = conversation.FindParticipant(memberId)!;
        participant.LastReadMessageSentAt.Should().Be(Now.AddMinutes(10));
        participant.LastReadMessageId.Should().Be(recente);
    }

    [Fact]
    public void Ler_deveria_implicar_ter_recebido()
    {
        // Arrange
        var (conversation, _, memberId) = Group();

        // Act
        conversation.MarkRead(memberId, Guid.NewGuid(), Now.AddMinutes(5));

        // Assert
        var participant = conversation.FindParticipant(memberId)!;
        participant.HasReceived(Now.AddMinutes(5)).Should().BeTrue();
        participant.HasRead(Now.AddMinutes(5)).Should().BeTrue();
    }

    [Fact]
    public void Nao_deveria_retroceder_o_cursor_de_entrega()
    {
        // Arrange
        var (conversation, _, memberId) = Group();
        conversation.MarkDelivered(memberId, Now.AddMinutes(10));

        // Act
        conversation.MarkDelivered(memberId, Now.AddMinutes(1));

        // Assert
        conversation.FindParticipant(memberId)!.LastDeliveredMessageSentAt.Should().Be(Now.AddMinutes(10));
    }

    [Fact]
    public void Nao_deveria_considerar_lida_uma_mensagem_posterior_ao_cursor()
    {
        // Arrange
        var (conversation, _, memberId) = Group();
        conversation.MarkRead(memberId, Guid.NewGuid(), Now.AddMinutes(5));

        // Act
        var participant = conversation.FindParticipant(memberId)!;

        // Assert
        participant.HasRead(Now.AddMinutes(6)).Should().BeFalse();
        participant.HasRead(Now.AddMinutes(5)).Should().BeTrue();
    }
}
