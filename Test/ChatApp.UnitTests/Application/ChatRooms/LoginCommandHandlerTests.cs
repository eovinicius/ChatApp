using System.Threading.Tasks;

using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Users.Login;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.ChatRooms;

public class LoginCommandHandlerTests
{
    private static readonly LoginCommand Command = new("username", "password");

    private readonly LoginCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IHashService _hashServiceMock;
    private readonly IAuthenticationService _authenticationServiceMock;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _authenticationServiceMock = Substitute.For<IAuthenticationService>();
        _hashServiceMock = Substitute.For<IHashService>();

        _handler = new LoginCommandHandler(
            _userRepositoryMock,
            _authenticationServiceMock,
            _hashServiceMock);
    }
    [Fact]
    public async Task Handle_DeveAutenticarUsuario_QuandoCredenciaisForemValidas()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        var token = "jwt_token";

        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(user);
        _hashServiceMock.Compare(Command.Password, user.Password).Returns(true);
        _authenticationServiceMock.GenerateToken(user).Returns(token);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(token);

        await _userRepositoryMock.Received(1).GetByUsername(Command.Username, Arg.Any<CancellationToken>());
        _hashServiceMock.Received(1).Compare(Command.Password, user.Password);
        _authenticationServiceMock.Received(1).GenerateToken(user);
    }

    [Fact]
    public async Task Handle_NaoDeveAutenticarUsuario_QuandoUsernameEstiverIncorreto()
    {
        // Arrange
        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        await _userRepositoryMock.Received(1).GetByUsername(Command.Username, Arg.Any<CancellationToken>());
        _hashServiceMock.DidNotReceive().Compare(Arg.Any<string>(), Arg.Any<string>());
        _authenticationServiceMock.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_NaoDeveAutenticarUsuario_QuandoSenhaEstiverIncorreta()
    {
        // Arrange
        var user = new User("John Doe", "username", "password");
        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(user);
        _hashServiceMock.Compare(Command.Password, user.Password).Returns(false);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        await _userRepositoryMock.Received(1).GetByUsername(Command.Username, Arg.Any<CancellationToken>());
        _hashServiceMock.Received(1).Compare(Command.Password, user.Password);
        _authenticationServiceMock.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

}
