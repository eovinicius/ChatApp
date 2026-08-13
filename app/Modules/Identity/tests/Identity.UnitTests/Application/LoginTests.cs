using FluentAssertions;

using Identity.Application.Abstractions;
using Identity.Application.UseCases.Users.Login;
using Identity.Domain.Entities.Users;
using Identity.Domain.Repositories;

using NSubstitute;

namespace Identity.UnitTests.Application;

public class LoginTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IAuthenticationService _authenticationServiceMock = Substitute.For<IAuthenticationService>();
    private readonly IHashService _hashServiceMock = Substitute.For<IHashService>();

    private readonly LoginCommandHandler _handler;

    private static readonly LoginCommand Command = new("johndoe", "senha123");

    public LoginTests()
    {
        _handler = new LoginCommandHandler(_userRepositoryMock, _authenticationServiceMock, _hashServiceMock);
    }

    [Fact]
    public async Task Deveria_autenticar_com_credenciais_validas()
    {
        // Arrange
        var user = User.Create("John Doe", "johndoe", "hash").Value;
        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(user);
        _hashServiceMock.Compare(Command.Password, "hash").Returns(true);
        _authenticationServiceMock.GenerateToken(user).Returns("token");

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("token");
    }

    [Fact]
    public async Task Nao_deveria_autenticar_usuario_inexistente()
    {
        // Arrange
        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
        _authenticationServiceMock.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Nao_deveria_autenticar_com_senha_incorreta()
    {
        // Arrange
        var user = User.Create("John Doe", "johndoe", "hash").Value;
        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(user);
        _hashServiceMock.Compare(Command.Password, "hash").Returns(false);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
        _authenticationServiceMock.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}
