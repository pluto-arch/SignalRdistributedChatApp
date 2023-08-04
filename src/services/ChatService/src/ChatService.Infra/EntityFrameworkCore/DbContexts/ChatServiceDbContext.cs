using ChatService.Domain.Aggregates.System;
using ChatService.Infra.EntityFrameworkCore.EntityTypeConfig;
using ChatService.Uow;


namespace ChatService.Infra.EntityFrameworkCore.DbContexts;

public class ChatServiceDbContext : BaseDbContext<ChatServiceDbContext>, IDataContext
{
    public ChatServiceDbContext(DbContextOptions<ChatServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<PermissionGrant> PermissionGrants { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // 不能去除，对租户，软删除过滤器
        modelBuilder
            .ApplyConfiguration(new PermissionEntityTypeConfiguration());
    }
}