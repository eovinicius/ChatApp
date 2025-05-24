using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Mapping;

public class ChatRoomUserConfiguration : IEntityTypeConfiguration<ChatRoomUser>
{
    public void Configure(EntityTypeBuilder<ChatRoomUser> builder)
    {
        builder.HasKey(cru => new { cru.ChatRoomId, cru.UserId });

        builder.Property(cru => cru.JoinedAt)
            .IsRequired();

        builder.Property(cru => cru.IsAdmin)
            .IsRequired();

        builder.HasOne<ChatRoom>()
            .WithMany("_members")
            .HasForeignKey(cru => cru.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
           .WithMany()
           .HasForeignKey(cru => cru.UserId);
    }
}
