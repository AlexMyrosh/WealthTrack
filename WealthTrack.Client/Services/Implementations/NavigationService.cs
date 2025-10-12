using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Services.Implementations;

public class NavigationService : INavigationService
{
    public async Task GoToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }
}