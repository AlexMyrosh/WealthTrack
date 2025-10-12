namespace WealthTrack.Client.Services.Interfaces;

public interface INavigationService
{
    Task GoToAsync(string route);
}