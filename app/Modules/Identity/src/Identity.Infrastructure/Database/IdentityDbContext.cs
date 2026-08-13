using Identity.Domain.Entities.Users;

using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Database;

public class IdentityDbContext : DbContext
{
    public const string Schema = "identity";

    public DbSet<User> Users { get; set; } = null!;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
