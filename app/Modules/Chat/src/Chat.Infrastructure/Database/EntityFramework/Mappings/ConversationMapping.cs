using Chat.Domain.Conversations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chat.Infrastructure.Database.EntityFramework.Mappings;

public class ConversationMapping : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(Conversation.MaxNameLength);

        builder.Property(c => c.AvatarUrl)
            .HasMaxLength(2048);

        builder.Property(c => c.DirectKey)
            .HasMaxLength(73); // dois GUIDs "D" (36) + ':'

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.LastActivityAt).IsRequired();
        builder.Property(c => c.MaxParticipants).IsRequired();

        // É este índice que garante, no banco, uma única conversa 1x1 por par de usuários.
        // Grupos têm DirectKey nulo e ficam de fora do índice.
        builder.HasIndex(c => c.DirectKey)
            .IsUnique()
            .HasFilter("\"DirectKey\" IS NOT NULL");

        // Ordenação da lista de conversas (tela inicial).
        builder.HasIndex(c => c.LastActivityAt)
            .IsDescending();

        builder
            .HasMany(c => c.Participants)
            .WithOne()
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(c => c.Participants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
