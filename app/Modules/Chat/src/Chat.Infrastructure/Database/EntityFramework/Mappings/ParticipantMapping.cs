using Chat.Domain.Conversations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chat.Infrastructure.Database.EntityFramework.Mappings;

public class ParticipantMapping : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");

        // Chave própria (herdada de Entity) + índice único no par lógico. Evita o
        // Ignore(Id) que o modelo antigo precisava por nunca gerar Id.
        builder.HasKey(p => p.Id);

        // Quem gera o Id é o domínio, no construtor. Sem isto o EF assume geração pelo
        // banco e, ao anexar um participante novo a uma conversa já rastreada, interpreta
        // "Id preenchido = registro existente" e emite UPDATE em vez de INSERT.
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.HasIndex(p => new { p.ConversationId, p.UserId })
            .IsUnique();

        builder.Property(p => p.ConversationId).IsRequired();
        builder.Property(p => p.UserId).IsRequired();

        builder.Property(p => p.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.JoinedAt).IsRequired();
        builder.Property(p => p.LeftAt);
        builder.Property(p => p.LastReadMessageSentAt);
        builder.Property(p => p.LastReadMessageId);
        builder.Property(p => p.LastDeliveredMessageSentAt);
        builder.Property(p => p.IsMuted).IsRequired();

        // "Minhas conversas" parte sempre do usuário.
        builder.HasIndex(p => new { p.UserId, p.LeftAt });
    }
}
