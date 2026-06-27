using ChatApp.Domain.Entities.Users;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.Users;

public class UserTest
{
    [Fact]
    public void Deveria_criar_usuario_com_dados_validos()
    {
        var result = User.Create("João Silva", "joaosilva", "hash123");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("João Silva");
        result.Value.Username.Should().Be("joaosilva");
        result.Value.Password.Should().Be("hash123");
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Nao_deveria_criar_usuario_com_nome_vazio()
    {
        var result = User.Create("   ", "joaosilva", "hash123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmptyName");
    }

    [Fact]
    public void Nao_deveria_criar_usuario_com_username_vazio()
    {
        var result = User.Create("João Silva", "", "hash123");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.EmptyUsername");
    }

    [Fact]
    public void Deveria_gerar_ids_unicos_para_cada_usuario()
    {
        var user1 = User.Create("User A", "usera", "pass").Value;
        var user2 = User.Create("User B", "userb", "pass").Value;

        user1.Id.Should().NotBe(user2.Id);
    }
}
