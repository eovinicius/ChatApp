using BuildingBlocks.Authentication;

using Chat.Application.Abstractions.Data;
using Chat.Application.UseCases.Conversations.MarkAsRead;
using Chat.Domain.Conversations;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace Chat.UnitTests.Application.Conversations;

public class MarkConversationAsReadTests
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    private readonly IConversationRepository _conversationRepositoryMock = Substitute.For<IConversationRepository>();
    private readonly IMessageRepository _messageRepositoryMock = Substitute.For<IMessageRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();

    private readonly MarkConversationAsReadCommandHandler _handler;

    private readonly Guid _readerId = Guid.NewGuid();
    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Conversation _conversation;
    private readonly Message _message;

    public MarkConversationAsReadTests()
    {
        _handler = new MarkConversationAsReadCommandHandler(
            _conversationRepositoryMock,
            _messageRepositoryMock,
            _unitOfWorkMock,
            _userContextMock);

        _conversation = Conversation.CreateDirect(_readerId, _senderId, Now).Value;
        _message = Message.Create(_conversation.Id, _senderId, MessageContent.CreateText("oi").Value, Now.AddMinutes(5)).Value;

        _userContextMock.UserId.Returns(_readerId);

        _conversationRepositoryMock
            .GetByIdWithParticipants(_conversation.Id, Arg.Any<CancellationToken>())
            .Returns(_conversation);

        _messageRepositoryMock
            .GetById(_message.Id, Arg.Any<CancellationToken>())
            .Returns(_message);
    }

    [Fact]
    public async Task Deveria_avancar_o_cursor_com_o_sentat_da_mensagem()
    {
        // Act
        var result = await _handler.Handle(new MarkConversationAsReadCommand(_conversation.Id, _message.Id), CancellationToken.None);

        // Assert — o cursor guarda o SentAt, não o instante do ack.
        result.IsSuccess.Should().BeTrue();
        _conversation.FindParticipant(_readerId)!.LastReadMessageSentAt.Should().Be(_message.SentAt);
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_marcar_leitura_com_mensagem_de_outra_conversa()
    {
        // Arrange
        var outra = Message.Create(Guid.NewGuid(), _senderId, MessageContent.CreateText("oi").Value, Now).Value;
        _messageRepositoryMock.GetById(outra.Id, Arg.Any<CancellationToken>()).Returns(outra);

        // Act
        var result = await _handler.Handle(new MarkConversationAsReadCommand(_conversation.Id, outra.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.WrongConversation");
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_marcar_leitura_em_conversa_inexistente()
    {
        // Arrange
        _conversationRepositoryMock
            .GetByIdWithParticipants(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        // Act
        var result = await _handler.Handle(new MarkConversationAsReadCommand(Guid.NewGuid(), _message.Id), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotFound");
    }
}
