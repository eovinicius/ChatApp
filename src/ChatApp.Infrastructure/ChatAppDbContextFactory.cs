using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatApp.Infrastructure;

public class ChatAppDbContextFactory : IDesignTimeDbContextFactory<ChatAppDbContext>
{
    public ChatAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatAppDbContext>();
        var connectionString = "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres;";
        optionsBuilder.UseNpgsql(connectionString);

        return new ChatAppDbContext(optionsBuilder.Options);
    }
}
