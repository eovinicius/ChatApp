using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;

using Chat.Application.Abstractions.Data;
using Chat.Application.UseCases.Messages.SendMessage;
using Chat.Domain.Conversations;
using Chat.Domain.Messages;
using Chat.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace Chat.UnitTests.Application.Messages;

public class SendMessageTests
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    private readonly IConversationRepository _conversationRepositoryMock = Substitute.For<IConversationRepository>();
    private readonly IMessageRepository _messageRepositoryMock = Substitute.For<IMessageRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
    private readonly IDateTimeProvider _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

    private readonly SendMessageCommandHandler _handler;

    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Guid _otherId = Guid.NewGuid();
    private readonly Conversation _conversation;

    public SendMessageTests()
    {
        _handler = new SendMessageCommandHandler(
            _conversationRepositoryMock,
            _messageRepositoryMock,
            _unitOfWorkMock,
            _userContextMock,
            _dateTimeProviderMock);

        _conversation = Conversation.CreateDirect(_senderId, _otherId, Now).Value;

        _userContextMock.UserId.Returns(_senderId);
        _dateTimeProviderMock.UtcNow.Returns(Now.AddMinutes(5));

        _conversationRepositoryMock
            .GetByIdWithParticipants(_conversation.Id, Arg.Any<CancellationToken>())
            .Returns(_conversation);
    }

    private SendMessageCommand TextCommand => new(_conversation.Id, "text", "olá");

    [Fact]
    public async Task Deveria_enviar_mensagem_de_texto()
    {
        // Act
        var result = await _handler.Handle(TextCommand, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _messageRepositoryMock.Received(1).Add(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deveria_atualizar_a_ultima_atividade_da_conversa()
    {
        // Act
        await _handler.Handle(TextCommand, CancellationToken.None);

        // Assert — é o que mantém a lista de conversas ordenada.
        _conversation.LastActivityAt.Should().Be(Now.AddMinutes(5));
    }

    [Fact]
    public async Task Deveria_marcar_a_propria_mensagem_como_lida_pelo_remetente()
    {
        // Act
        await _handler.Handle(TextCommand, CancellationToken.None);

        // Assert — quem envia não deve ver a própria mensagem como não lida.
        _conversation.FindParticipant(_senderId)!.LastReadMessageSentAt.Should().Be(Now.AddMinutes(5));
        _conversation.FindParticipant(_otherId)!.LastReadMessageSentAt.Should().BeNull();
    }

    [Fact]
    public async Task Nao_deveria_enviar_para_conversa_inexistente()
    {
        // Arrange
        _conversationRepositoryMock
            .GetByIdWithParticipants(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        // Act
        var result = await _handler.Handle(new SendMessageCommand(Guid.NewGuid(), "text", "olá"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotFound");
    }

    [Fact]
    public async Task Nao_deveria_enviar_se_nao_participa_da_conversa()
    {
        // Arrange
        _userContextMock.UserId.Returns(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(TextCommand, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
        await _messageRepositoryMock.DidNotReceive().Add(Arg.Any<Message>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_enviar_com_tipo_de_conteudo_invalido()
    {
        // Act
        var result = await _handler.Handle(new SendMessageCommand(_conversation.Id, "sticker", "x"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidContentType");
    }

    [Fact]
    public async Task Nao_deveria_enviar_midia_sem_chave_de_storage()
    {
        // Act
        var result = await _handler.Handle(
            new SendMessageCommand(_conversation.Id, "image", "https://cdn/x.png"),
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.MissingStorageKey");
    }
}
