using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.ChatRooms;

public class ChatRoomTest
{
    [Fact]
    public void Deveria_criar_sala_publica_sem_senha()
    {
        var name = "sala";
        var user = new User("John Doe");
        var isPrivate = false;

        var room = ChatRoom.Create(name, user, isPrivate);

        room.Should().NotBeNull();
        room.Id.Should().NotBe(Guid.Empty);
        room.Members.Should().HaveCount(1);
        room.Members.First().UserId.Should().Be(user.Id);
        room.Members.Last().ChatRoomId.Should().Be(room.Id);
        room.Password.Should().Be(string.Empty);
    }

    [Fact]
    public void Deveria_criar_sala_privada_com_senha()
    {
        // Arrange
        var user = new User("John Doe");
        var name = "sala";
        var isPrivate = true;
        var senha = "123";

        // Act
        var room = ChatRoom.Create(name, user, isPrivate, senha);

        // Assert
        room.Should().NotBeNull();
        room.Id.Should().NotBe(Guid.Empty);
        room.Members.Should().HaveCount(1);
        room.Members.First().UserId.Should().Be(user.Id);
        room.Members.Last().ChatRoomId.Should().Be(room.Id);
        room.Password.Should().NotBeNull();
        room.Password.Should().Be(senha);
    }

    [Fact]
    public void Deveria_adicionar_usuario_quando_entrar_na_sala_publica()
    {
        // Arrange
        var user = new User("John Doe");
        var name = "sala";
        var isPrivate = false;
        var senha = "123";


        var user2 = new User("jose");

        // Act
        var room = ChatRoom.Create(name, user, isPrivate, senha);
        room.Join(user2);

        // Assert
        room.Should().NotBeNull();
        room.Members.Should().HaveCount(2);
        room.Members.Last().UserId.Should().Be(user2.Id);
        room.Members.Last().ChatRoomId.Should().Be(room.Id);
    }

    [Fact]
    public void Deveria_remover_usuario_da_lista_de_membros_ao_sair_da_sala()
    {
        // Arrange
        var user = new User("John Doe");
        var user2 = new User("jose");
        var name = "sala";
        var isPrivate = false;
        var senha = "123";

        var room = ChatRoom.Create(name, user, isPrivate, senha);
        room.Join(user2);

        // Act
        room.Leave(user2);

        // Assert
        room.Members.Should().HaveCount(1);
        room.Members.Should().OnlyContain(m => m.UserId == user.Id);
        room.Members.Any(m => m.UserId == user2.Id).Should().BeFalse();
    }
}