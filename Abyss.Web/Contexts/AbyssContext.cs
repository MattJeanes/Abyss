using Abyss.Web.Entities;
using Microsoft.EntityFrameworkCore;

namespace Abyss.Web.Contexts;

public class AbyssContext : DbContext
{
    public AbyssContext(DbContextOptions<AbyssContext> options) : base(options)
    {

    }

    public DbSet<GPTModel> GPTModels { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Server> Servers { get; set; }
    public DbSet<User> Users { get; set; }
}
