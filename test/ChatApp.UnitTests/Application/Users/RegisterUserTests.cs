using ChatApp.Application.Abstractions.Authentication;
using ChatApp.Application.Abstractions.Data;
using ChatApp.Application.Abstractions.Services;
using ChatApp.Application.UseCases.Users.RegisterUser;
using ChatApp.Domain.Entities.Users;
using ChatApp.Domain.Repositories;

using FluentAssertions;

using NSubstitute;

namespace ChatApp.UnitTests.Application.Users;

public class RegisterUserTests
{
    private static readonly RegisterUserCommand Command = new("John Doe", "username", "password123");

    private readonly RegisterUserCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;
    private readonly IHashService _hashServiceMock;
    private readonly IAuthenticationService _authenticationServiceMock;

    public RegisterUserTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();
        _hashServiceMock = Substitute.For<IHashService>();
        _authenticationServiceMock = Substitute.For<IAuthenticationService>();

        _handler = new RegisterUserCommandHandler(
            _userRepositoryMock,
            _unitOfWorkMock,
            _hashServiceMock,
            _authenticationServiceMock);
    }

    [Fact]
    public async Task Handle_Deve_Registrar_Usuario_Com_Sucesso()
    {
        // Arrange
        var hashedPassword = "hashed_password";
        var token = "jwt_token";

        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns((User?)null);
        _hashServiceMock.Hash(Command.Password).Returns(hashedPassword);
        _authenticationServiceMock.GenerateToken(Arg.Any<User>()).Returns(token);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(token);

        await _userRepositoryMock.Received(1).GetByUsername(Command.Username, Arg.Any<CancellationToken>());
        _hashServiceMock.Received(1).Hash(Command.Password);
        await _userRepositoryMock.Received(1).Add(
            Arg.Is<User>(u => u.Name == Command.Name && u.Username == Command.Username && u.Password == hashedPassword),
            Arg.Any<CancellationToken>());
        await _unitOfWorkMock.Received(1).Commit(Arg.Any<CancellationToken>());
        _authenticationServiceMock.Received(1).GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_Deve_Retornar_Erro_Quando_Username_Ja_Existe()
    {
        // Arrange
        var existingUser = new User("Existing User", Command.Username, "password");
        _userRepositoryMock.GetByUsername(Command.Username, Arg.Any<CancellationToken>()).Returns(existingUser);

        // Act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();

        await _userRepositoryMock.Received(1).GetByUsername(Command.Username, Arg.Any<CancellationToken>());
        _hashServiceMock.DidNotReceive().Hash(Arg.Any<string>());
        await _userRepositoryMock.DidNotReceive().Add(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWorkMock.DidNotReceive().Commit(Arg.Any<CancellationToken>());
        _authenticationServiceMock.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}

