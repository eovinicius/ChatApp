using ChatApp.Domain.Entities.ChatRooms;

using FluentAssertions;

namespace ChatApp.UnitTests.Domain.ChatRooms;

public class ChatRoomUserTest
{
    [Fact]
    public void Deveria_criar_membro_autenticado()
    {
        var chatRoomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var member = ChatRoomUser.Create(chatRoomId, userId);

        member.ChatRoomId.Should().Be(chatRoomId);
        member.UserId.Should().Be(userId);
        member.IsAdmin.Should().BeFalse();
        member.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Deveria_promover_membro_a_admin()
    {
        var member = ChatRoomUser.Create(Guid.NewGuid(), Guid.NewGuid());

        member.PromoteToAdmin();

        member.IsAdmin.Should().BeTrue();
    }

    [Fact]
    public void Promover_a_admin_duas_vezes_deve_manter_admin_true()
    {
        var member = ChatRoomUser.Create(Guid.NewGuid(), Guid.NewGuid());

        member.PromoteToAdmin();
        member.PromoteToAdmin();

        member.IsAdmin.Should().BeTrue();
    }
}
