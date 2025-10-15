using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;


public partial class SyncSelectionViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IWalletService _walletService;
    private readonly ICurrencyService _currencyService;
    
    private readonly INavigation _navigation;
    
    [ObservableProperty] private bool _isSyncEnabled;
    [ObservableProperty] private int _syncIndex;

    public IAsyncRelayCommand NextCommand { get; }

    public SyncSelectionViewModel(IUserService userService, IWalletService walletService, ICurrencyService currencyService, INavigation navigation)
    {
        _userService = userService;
        _walletService = walletService;
        _currencyService = currencyService;
        
        _navigation = navigation;

        NextCommand = new AsyncRelayCommand(OnNextAsync);

        _ = LoadDataAsync();
    }
    
    partial void OnSyncIndexChanged(int value)
    {
        IsSyncEnabled = value == 0; // 0 - On, 1 - Off
    }

    private async Task LoadDataAsync()
    {
        var userSession = await _userService.GetUserSessionAsync() ?? new UserSession();

        if (userSession.IsSyncEnabled.HasValue)
        {
            IsSyncEnabled = userSession.IsSyncEnabled.Value;
        }
    }

    private async Task OnNextAsync()
    {
        var userSession = await _userService.GetUserSessionAsync() ?? new UserSession();
        userSession.IsSyncEnabled = IsSyncEnabled;
        await _userService.SaveUserSessionAsync(userSession);
        await _navigation.PushAsync(new InitialWalletCreationPage(_walletService, _currencyService, _userService));
    }
}