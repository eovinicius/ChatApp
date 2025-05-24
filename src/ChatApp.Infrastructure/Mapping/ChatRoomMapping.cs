using ChatApp.Domain.Entities.ChatRooms;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Mapping;

public class ChatRoomMapping : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        {
            builder.ToTable("ChatRooms");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Password)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.OwnerId)
                .IsRequired();

            builder.Property(c => c.IsPrivate)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.HasMany(typeof(ChatRoomUser), "_members")
                .WithOne()
                .HasForeignKey("ChatRoomId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Configura a propriedade _members como campo privado (backing field)
            builder.Navigation(nameof(ChatRoom.Members))
                .UsePropertyAccessMode(PropertyAccessMode.Field);

        }
    }

}