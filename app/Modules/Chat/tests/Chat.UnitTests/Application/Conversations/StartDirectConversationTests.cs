using BuildingBlocks.Authentication;
using BuildingBlocks.Clock;

using Chat.Application.Abstractions.Data;
using Chat.Application.UseCases.Conversations.StartDirectConversation;
using Chat.Domain.Conversations;
using Chat.Domain.Repositories;

using FluentAssertions;

using Identity.Contracts;

using NSubstitute;

namespace Chat.UnitTests.Application.Conversations;

public class StartDirectConversationTests
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    private readonly IConversationRepository _conversationRepositoryMock = Substitute.For<IConversationRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IUserContext _userContextMock = Substitute.For<IUserContext>();
    private readonly IUserDirectory _userDirectoryMock = Substitute.For<IUserDirectory>();
    private readonly IDateTimeProvider _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();

    private readonly StartDirectConversationCommandHandler _handler;

    private readonly Guid _currentUserId = Guid.NewGuid();
    private readonly Guid _targetUserId = Guid.NewGuid();

    public StartDirectConversationTests()
    {
        _handler = new StartDirectConversationCommandHandler(
            _conversationRepositoryMock,
            _unitOfWorkMock,
            _userContextMock,
            _userDirectoryMock,
            _dateTimeProviderMock);

        _userContextMock.UserId.Returns(_currentUserId);
        _dateTimeProviderMock.UtcNow.Returns(Now);
        _userDirectoryMock.Exists(_targetUserId, Arg.Any<CancellationToken>()).Returns(true);
    }

    [Fact]
    public async Task Deveria_criar_conversa_quando_ainda_nao_existe()
    {
        // Arrange
        _conversationRepositoryMock
            .GetDirectByKey(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        // Act
        var result = await _handler.Handle(new StartDirectConversationCommand(_targetUserId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _conversationRepositoryMock.Received(1).Add(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deveria_devolver_a_conversa_existente_sem_criar_outra()
    {
        // Arrange
        var existing = Conversation.CreateDirect(_currentUserId, _targetUserId, Now).Value;

        _conversationRepositoryMock
            .GetDirectByKey(Conversation.BuildDirectKey(_currentUserId, _targetUserId), Arg.Any<CancellationToken>())
            .Returns(existing);

        // Act
        var result = await _handler.Handle(new StartDirectConversationCommand(_targetUserId), CancellationToken.None);

        // Assert — idempotência: mesmo id, nenhuma escrita.
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing.Id);
        await _conversationRepositoryMock.DidNotReceive().Add(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_iniciar_conversa_consigo_mesmo()
    {
        // Act
        var result = await _handler.Handle(new StartDirectConversationCommand(_currentUserId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.DirectWithSelf");
    }

    [Fact]
    public async Task Nao_deveria_iniciar_conversa_com_usuario_inexistente()
    {
        // Arrange
        var desconhecido = Guid.NewGuid();
        _userDirectoryMock.Exists(desconhecido, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(new StartDirectConversationCommand(desconhecido), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.TargetUserNotFound");
        await _conversationRepositoryMock.DidNotReceive().Add(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
    }
}
