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
        Page rootPage;

        var token = SecureStorage.GetAsync("auth_token").Result;
        if (!string.IsNullOrEmpty(token))
        {
            rootPage = new AppShell();
        }
        else
        {
            rootPage = new LoginPage(_authService);
        }

        return new Window(rootPage);
    }
}