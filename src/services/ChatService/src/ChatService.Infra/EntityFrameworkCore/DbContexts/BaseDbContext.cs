using System.Data;
using ChatService.Domain.Infra;
using ChatService.Infra.EntityFrameworkCore.Extension;
using ChatService.Uow;
using Microsoft.EntityFrameworkCore.Storage;


namespace ChatService.Infra.EntityFrameworkCore.DbContexts;

public abstract class BaseDbContext<TContext> : DbContext where TContext : DbContext, IDataContext
{
    protected BaseDbContext(DbContextOptions<TContext> options)
        : base(options)
    {
    }



    public IDbConnection DbConnection => this.Database.GetDbConnection();

    public IDbTransaction DbTransaction => this.Database.CurrentTransaction?.GetDbTransaction();

    public int? CommandTimeOut
    {
        get => this.Database.GetCommandTimeout();
        set => this.Database.SetCommandTimeout(value);
    }


    public ILogger GetLogger<TSourceContext>()
    {
        return this.GetService<ILogger<TSourceContext>>();
    }

    public bool HasChanges()
    {
        return this.ChangeTracker.HasChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType item in modelBuilder.Model.GetEntityTypes())
        {
            // 软删除
            if (item.ClrType.IsAssignableTo(typeof(ISoftDelete)))
            {
                modelBuilder.Entity(item.ClrType).AddQueryFilter<ISoftDelete>(e => !e.Deleted);
            }
        }
    }
}