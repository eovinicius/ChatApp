using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Clock;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Storage;
using ChatApp.Application.UseCases.Messages.DeleteMessage;
using ChatApp.Domain.Entities.Messages;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace ChatApp.UnitTests.Application.Messages;

public class DeleteMessageTests
{
    private static readonly DeleteMessageCommand Command = new(
        Guid.NewGuid(),
        Guid.NewGuid()
);

    private readonly DeleteMessageCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserContext _userContextMock;
    private readonly IChatMessageRepository _chatMessageRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IDateTimeProvider _dateTimeProviderMock;
    private readonly IFileStorageService _fileStorageServiceMock;

    public DeleteMessageTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userContextMock = Substitute.For<IUserContext>();
        _chatMessageRepositoryMock = Substitute.For<IChatMessageRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        _fileStorageServiceMock = Substitute.For<IFileStorageService>();

        _handler = new DeleteMessageCommandHandler(
            _chatMessageRepositoryMock,
            _unitOfWorkMock,
            _userContextMock,
            _dateTimeProviderMock,
            _userRepositoryMock,
            _fileStorageServiceMock
        );
    }


    [Fact]
    public async Task Deveria_deletar_mensagem_com_sucesso()
    {
        var user = new User("John Doe", "username", "password");
        var roomId = Guid.NewGuid();
        var message = new ChatMessage(roomId, ContentType.Text, "test", user.Id, DateTime.UtcNow);

        _userContextMock.UserId.Returns(user.Id);
        _dateTimeProviderMock.UtcNow.Returns(DateTime.UtcNow);
        _userRepositoryMock.GetById(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _chatMessageRepositoryMock.GetById(message.Id, Arg.Any<CancellationToken>()).Returns(message);

        var command = new DeleteMessageCommand(message.Id, roomId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
        _chatMessageRepositoryMock.Received(1).Delete(message, Arg.Any<CancellationToken>());
    }
}
