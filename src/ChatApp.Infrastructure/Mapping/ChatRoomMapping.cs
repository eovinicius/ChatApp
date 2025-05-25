using ChatApp.Domain.Entities.ChatRooms;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Mapping;

public class ChatRoomMapping : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Password)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(c => c.OwnerId)
            .IsRequired();

        builder.Property(c => c.IsPrivate)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Metadata
            .FindNavigation(nameof(ChatRoom.Members))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
