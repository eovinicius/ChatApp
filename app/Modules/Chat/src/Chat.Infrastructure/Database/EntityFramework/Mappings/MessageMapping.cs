using Chat.Domain.Messages;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chat.Infrastructure.Database.EntityFramework.Mappings;

public class MessageMapping : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.ConversationId).IsRequired();
        builder.Property(m => m.SenderId).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();
        builder.Property(m => m.EditedAt);
        builder.Property(m => m.DeletedAt);

        builder.OwnsOne(m => m.Content, content =>
        {
            content.Property(c => c.Type)
                .HasColumnName("ContentType")
                .HasConversion(
                    type => type.Value,
                    value => ContentType.From(value).Value)
                .HasMaxLength(20)
                .IsRequired();

            content.Property(c => c.Value)
                .HasColumnName("Content")
                .IsRequired();

            content.Property(c => c.StorageKey)
                .HasColumnName("StorageKey")
                .HasMaxLength(512);

            content.Property(c => c.FileName)
                .HasColumnName("FileName")
                .HasMaxLength(255);

            content.Property(c => c.SizeBytes)
                .HasColumnName("SizeBytes");
        });

        builder.Navigation(m => m.Content).IsRequired();

        // Paginação keyset do histórico: o Id no fim é o desempate que faltava e que
        // fazia mensagens com o mesmo SentAt sumirem ou repetirem entre páginas.
        builder.HasIndex(m => new { m.ConversationId, m.SentAt, m.Id })
            .IsDescending(false, true, true);

        builder.HasIndex(m => m.SenderId);
    }
}
