
using ChatService.Domain.Infra;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChatService.Infra.EntityFrameworkCore.Interceptor;

public class DataChangeSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;

    public DataChangeSaveChangesInterceptor(IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }


    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData,
           InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            SoftDeleteTracking(eventData.Context);
            DispatchDomainEventsAsync(eventData.Context).Wait();
        }
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            SoftDeleteTracking(eventData.Context);
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void SoftDeleteTracking(DbContext dbContext)
    {
        var deletedEntries = dbContext.ChangeTracker.Entries()
            .Where(entry => entry.State == EntityState.Deleted && entry.Entity is ISoftDelete);
        deletedEntries?.ToList().ForEach(entityEntry =>
        {
            entityEntry.Reload();
            entityEntry.State = EntityState.Modified;
            ( (ISoftDelete)entityEntry.Entity ).Deleted = true;
        });
    }



    private async Task DispatchDomainEventsAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        var domainEntities = dbContext.ChangeTracker.Entries<IDomainEvents>().Select(e => e.Entity);
        var domainEvents = domainEntities.SelectMany(x => x.DomainEvents).ToList();
        domainEntities.ToList().ForEach(entity => entity.ClearDomainEvents());
        foreach (var domainEvent in domainEvents)
        {
            await _dispatcher.Dispatch(domainEvent, cancellationToken);
        }
    }

}