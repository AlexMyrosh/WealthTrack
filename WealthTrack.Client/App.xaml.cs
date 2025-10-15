using WealthTrack.Business.Seeders;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client;

public partial class App : Application
{
    private readonly IUserService _userService;
    private readonly CurrenciesSeeder _currenciesSeeder;

    public App(IUserService userService, CurrenciesSeeder currenciesSeeder)
    {
        InitializeComponent();
        _userService = userService;
        _currenciesSeeder = currenciesSeeder;
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