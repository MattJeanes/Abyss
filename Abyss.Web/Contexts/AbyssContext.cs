using Abyss.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Contexts;

public class AbyssContext(DbContextOptions<AbyssContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>()
            .HasMany(e => e.Permissions)
            .WithMany(e => e.Roles)
            .UsingEntity<RolePermission>();
    }

    public DbSet<GPTModel> GPTModels { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<User> Users { get; set; }
}
