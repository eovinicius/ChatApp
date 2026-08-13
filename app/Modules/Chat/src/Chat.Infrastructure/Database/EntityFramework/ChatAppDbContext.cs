using Chat.Domain.Conversations;
using Chat.Domain.Messages;

using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Database.EntityFramework;

public class ChatAppDbContext : DbContext
{
    public const string Schema = "chat";

    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    public ChatAppDbContext(DbContextOptions<ChatAppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatAppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
