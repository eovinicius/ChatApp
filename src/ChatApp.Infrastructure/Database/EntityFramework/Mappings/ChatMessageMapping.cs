using ChatApp.Domain.Entities.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatApp.Infrastructure.Database.EntityFramework.Mappings;

public class ChatMessageMapping : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ChatRoomId).IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.SenderId).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();
        builder.Property(m => m.EditedAt);

        builder.Property(m => m.ContentType)
            .HasConversion(
                v => v.ToString(),
                v => ContentType.From(v))
            .IsRequired()
            .HasColumnName("ContentType");
    }
}
