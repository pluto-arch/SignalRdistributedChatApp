using ChatService.Infra.EntityFrameworkCore.EntityTypeConfig;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatService.Infra.EntityFrameworkCore.DbContexts;


public class DbContextDesignTimeFactory : IDesignTimeDbContextFactory<ChatServiceMigrationDbContext>
{
    public ChatServiceMigrationDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ChatServiceMigrationDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(@"Server=192.168.0.126,1433;Database=Pnct_Default;User Id=sa;Password=970307lBx;Trusted_Connection = False;TrustServerCertificate=true");
        return new ChatServiceMigrationDbContext(optionsBuilder.Options);
    }
}


/// <summary>
/// ef迁移使用
/// </summary>
public class ChatServiceMigrationDbContext : DbContext
{
    public ChatServiceMigrationDbContext(DbContextOptions<ChatServiceMigrationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .ApplyConfiguration(new PermissionEntityTypeConfiguration());
    }
}
