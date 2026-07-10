using Chat.Domain.Entities.ChatRooms;
using Chat.Domain.Entities.Messages;
using Chat.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Database.EntityFramework;

public class ChatAppDbContext : DbContext
{
    public DbSet<ChatRoom> ChatRooms { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ChatMessage> Messages { get; set; } = null!;

    public ChatAppDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChatAppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}