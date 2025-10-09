using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views;

namespace WealthTrack.Client;

public partial class App : Application
{
    private readonly IAuthService _authService;

    public App(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        return window;
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await InitializeAppAsync();
    }

    private async Task InitializeAppAsync()
    {
        // Start with loading page
        await Shell.Current.GoToAsync("//LoadingPage");
        
        var session = await _authService.GetUserSessionAsync();

        if (session != null)
            await Shell.Current.GoToAsync("//MainPage");
        else
            await Shell.Current.GoToAsync("//LoginPage");
    }
}