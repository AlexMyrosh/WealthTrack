using WealthTrack.Business.Seeders;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client;

public partial class App : Application
{
    private readonly IUserService _userService;
    private readonly CurrenciesSeeder _currenciesSeeder;
    private readonly ISyncService _syncService;

    public App(IUserService userService, CurrenciesSeeder currenciesSeeder, ISyncService syncService)
    {
        InitializeComponent();
        _userService = userService;
        _currenciesSeeder = currenciesSeeder;
        _syncService = syncService;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        return window;
    }

    protected override async void OnStart()
    {
        base.OnStart();
        _syncService.Start();
        await InitializeAppAsync();
    }
    
    protected override void OnSleep()
    {
        base.OnSleep();
        _syncService.Stop();
    }

    protected override void OnResume()
    {
        base.OnResume();
        _syncService.Start();
    }

    private async Task InitializeAppAsync()
    {
        await Shell.Current.GoToAsync("//LoadingPage");
        //await _currenciesSeeder.SeedAsync();
        
        var session = await _userService.GetUserSessionAsync();
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