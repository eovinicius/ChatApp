using FluentAssertions;

using Identity.Application.Abstractions;
using Identity.Application.UseCases.Users.RegisterUser;
using Identity.Domain.Entities.Users;
using Identity.Domain.Repositories;

using NSubstitute;

namespace Identity.UnitTests.Application;

public class RegisterUserTests
{
    private readonly IUserRepository _userRepositoryMock = Substitute.For<IUserRepository>();
    private readonly IIdentityUnitOfWork _unitOfWorkMock = Substitute.For<IIdentityUnitOfWork>();
    private readonly IHashService _hashServiceMock = Substitute.For<IHashService>();
    private readonly IAuthenticationService _authenticationServiceMock = Substitute.For<IAuthenticationService>();

    private readonly RegisterUserCommandHandler _handler;

    private static readonly RegisterUserCommand Command = new("John Doe", "johndoe", "senha123");

    public RegisterUserTests()
    {
        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _hashServiceMock,
            _authenticationServiceMock);
    }

    [Fact]
    public async Task Deveria_registrar_usuario_e_devolver_token()
    {
        // Arrange
        _userRepositoryMock.ExistsByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(false);
        _hashServiceMock.Hash(Command.Password).Returns("hash");
        _authenticationServiceMock.GenerateToken(Arg.Any<User>()).Returns("token");

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("token");
        await _userRepositoryMock.Received(1).Add(
            Arg.Is<User>(u => u.Username == Command.Username && u.Password == "hash"),
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_registrar_username_ja_em_uso()
    {
        // Arrange
        _userRepositoryMock.ExistsByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.UsernameAlreadyTaken");
        await _userRepositoryMock.DidNotReceive().Add(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_deveria_registrar_usuario_invalido()
    {
        // Arrange
        _userRepositoryMock.ExistsByUsername(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _hashServiceMock.Hash(Arg.Any<string>()).Returns("hash");

        // Act
        var result = await _handler.Handle(new RegisterUserCommand("", "johndoe", "senha123"), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmptyName");
    }
}
