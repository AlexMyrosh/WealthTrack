using WealthTrack.Client.Services.Interfaces;

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
        await Shell.Current.GoToAsync("//LoadingPage");
        
        var session = await _authService.GetUserSessionAsync();
        if (session is { IsIntroductionCompleted: true })
        {
            await Shell.Current.GoToAsync("//TransactionsPage");
        }
        else if (session is { IsIntroductionCompleted: false })
        {
            await Shell.Current.GoToAsync("//InitialCreationPage");
        }
        else
        {
            await Shell.Current.GoToAsync("//OnboardingPage");
        }
    }
}