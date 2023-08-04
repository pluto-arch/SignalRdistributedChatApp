using System.Transactions;

namespace ChatService.Application.IntegrationEvents.IntegrationEventbox;



/// <summary>
/// 发件箱
/// </summary>
[Injectable(InjectLifeTime.Transient)]
public partial class IntegrationEventBoxService
{
    public static readonly ConcurrentBag<string> InMemoryProductBox = new ConcurrentBag<string>();

    public void AddAndSaveEvent(string @event)
    {
        InMemoryProductBox.Add(@event);
    }


    public ConcurrentBag<string> MemoryBox => InMemoryProductBox;



    public async Task SetEventPublishing()
    {
        await Task.Yield();
    }


    public async Task SetEventPublished()
    {
        await Task.Yield();
    }
}