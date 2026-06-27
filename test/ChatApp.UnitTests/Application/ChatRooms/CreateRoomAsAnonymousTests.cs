using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Rooms.CreateRoomAsAnonymous;
using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.ChatRooms;

public class CreateRoomAsAnonymousTests
{
    private static readonly CreateRoomAsAnonymousCommand Command = new("sala-anonima", "Visitante");

    private readonly CreateRoomAsAnonymousCommandHandler _handler;
    private readonly IChatRoomRepository _chatRoomRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IChatHub _chatHubMock;

    public CreateRoomAsAnonymousTests()
    {
        _chatRoomRepositoryMock = Substitute.For<IChatRoomRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _chatHubMock = Substitute.For<IChatHub>();

        _handler = new CreateRoomAsAnonymousCommandHandler(
            _chatRoomRepositoryMock,
            _unitOfWorkMock,
            _chatHubMock);
    }

    [Fact]
    public async Task Handle_Deve_Criar_Sala_Anonima_E_Retornar_Id()
    {
        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_Deve_Persistir_Sala_Com_Nome_Correto()
    {
        // Act
        await _handler.Handle(Command, CancellationToken.None);

        // Assert
        await _chatRoomRepositoryMock.Received(1).Add(
            Arg.Is<ChatRoom>(r => r.Name == Command.RoomName && r.OwnerId == Guid.Empty && !r.IsPrivate),
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Adicionar_Convidado_Como_Membro_Anonimo()
    {
        // Act
        await _handler.Handle(Command, CancellationToken.None);

        // Assert
        await _chatRoomRepositoryMock.Received(1).Add(
            Arg.Is<ChatRoom>(r => r.Members.Any(m => m.GuestName == Command.GuestName)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Notificar_ChatHub_Com_Nome_Do_Convidado()
    {
        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        await _chatHubMock.Received(1).JoinGroup(
            result.Value.ToString(),
            Command.GuestName,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Nome_Da_Sala_Vazio()
    {
        var result = await _handler.Handle(new CreateRoomAsAnonymousCommand("   ", "Visitante"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ChatRoom.EmptyName");
        await _chatRoomRepositoryMock.DidNotReceive().Add(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Nome_Do_Convidado_Vazio()
    {
        var result = await _handler.Handle(new CreateRoomAsAnonymousCommand("sala", "   "), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ChatRoom.EmptyGuestName");
        await _chatRoomRepositoryMock.DidNotReceive().Add(Arg.Any<ChatRoom>(), Arg.Any<CancellationToken>());
    }
}
