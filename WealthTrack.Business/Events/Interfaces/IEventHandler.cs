namespace WealthTrack.Business.Events.Interfaces
{
    public interface IEventHandler<T>
    {
        Task Handle(T eventMessage);
    }
}
