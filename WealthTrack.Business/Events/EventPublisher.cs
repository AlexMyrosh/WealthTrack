using Microsoft.Extensions.DependencyInjection;
using WealthTrack.Business.Events.Interfaces;

namespace WealthTrack.Business.Events;

public class EventPublisher(IServiceProvider serviceProvider) : IEventPublisher
{
    public async Task PublishAsync<T>(T eventMessage)
    {
        var handlers = serviceProvider.GetServices<IEventHandler<T>>();
        foreach (var handler in handlers)
        {
            await handler.Handle(eventMessage);
        }
    }
}