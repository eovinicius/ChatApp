using ChatApp.Domain.Entities.ChatRooms;
using ChatApp.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Database.EntityFramework;

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