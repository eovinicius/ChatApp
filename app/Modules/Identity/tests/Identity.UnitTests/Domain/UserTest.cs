using FluentAssertions;

using Identity.Domain.Entities.Users;
using Identity.Domain.Events;

namespace Identity.UnitTests.Domain;

public class UserTest
{
    private static readonly DateTime Now = new(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Deveria_criar_usuario_e_disparar_evento()
    {
        // Act
        var result = User.Create("John Doe", "johndoe", "hash");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("John Doe");
        result.Value.Username.Should().Be("johndoe");
        result.Value.GetDomainEvents().Should().ContainSingle(e => e is UserRegisteredEvent);
    }

    [Theory]
    [InlineData("", "user", "pass", "User.EmptyName")]
    [InlineData("Nome", "", "pass", "User.EmptyUsername")]
    [InlineData("Nome", "user", "", "User.EmptyPassword")]
    public void Nao_deveria_criar_usuario_invalido(string name, string username, string password, string expectedCode)
    {
        // Act
        var result = User.Create(name, username, password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedCode);
    }

    [Fact]
    public void Deveria_renomear_usuario()
    {
        // Arrange
        var user = User.Create("John", "johndoe", "hash").Value;

        // Act
        var result = user.Rename("John Doe");

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Name.Should().Be("John Doe");
    }

    [Fact]
    public void Nao_deveria_renomear_para_vazio()
    {
        // Arrange
        var user = User.Create("John", "johndoe", "hash").Value;

        // Act
        var result = user.Rename("  ");

        // Assert
        result.IsFailure.Should().BeTrue();
        user.Name.Should().Be("John");
    }

    [Fact]
    public void Nao_deveria_retroceder_o_visto_por_ultimo()
    {
        // Arrange
        var user = User.Create("John", "johndoe", "hash").Value;
        user.TouchLastSeen(Now);

        // Act — desconexão atrasada chegando fora de ordem.
        user.TouchLastSeen(Now.AddMinutes(-10));

        // Assert
        user.LastSeenAt.Should().Be(Now);
    }

    [Fact]
    public void Deveria_avancar_o_visto_por_ultimo()
    {
        // Arrange
        var user = User.Create("John", "johndoe", "hash").Value;
        user.TouchLastSeen(Now);

        // Act
        user.TouchLastSeen(Now.AddMinutes(10));

        // Assert
        user.LastSeenAt.Should().Be(Now.AddMinutes(10));
    }
}
