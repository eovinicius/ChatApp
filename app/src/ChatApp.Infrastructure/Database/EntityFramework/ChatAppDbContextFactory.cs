using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatApp.Infrastructure.Database.EntityFramework;

public class ChatAppDbContextFactory : IDesignTimeDbContextFactory<ChatAppDbContext>
{
    public ChatAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatAppDbContext>();
        var connectionString =
            Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=chatapp;Username=postgres;Password=postgres;";
        optionsBuilder.UseNpgsql(connectionString);

        return new ChatAppDbContext(optionsBuilder.Options);
    }
}
