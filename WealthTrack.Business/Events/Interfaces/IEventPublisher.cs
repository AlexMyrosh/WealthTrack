namespace WealthTrack.Business.Events.Interfaces
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T eventMessage);
    }
}
