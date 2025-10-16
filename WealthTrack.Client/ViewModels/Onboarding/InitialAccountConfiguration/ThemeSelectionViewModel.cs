using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

public partial class ThemeSelectionViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly IThemeService _themeService;
    private readonly IWalletService _walletService;
    private readonly ICurrencyService _currencyService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IMapper _mapper;
    
    private readonly INavigation _navigation;
    
    [ObservableProperty] private ObservableCollection<CustomAppTheme> _themes = new();
    [ObservableProperty] private CustomAppTheme _selectedTheme;

    public ICommand NextCommand { get; }

    public ThemeSelectionViewModel(IUserService userService, IThemeService themeService, IWalletService walletService, ICurrencyService currencyService, INavigation navigation, INavigationService navigationService, IMapper mapper, IDialogService dialogService)
    {
        _userService = userService;
        _themeService = themeService;
        _walletService = walletService;
        _currencyService = currencyService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _mapper = mapper;

        _navigation = navigation;
        
        NextCommand = new AsyncRelayCommand(OnNextAsync);
        
        LoadData();
    }

    private void LoadData()
    {
        var userSession = _userService.GetUserSessionAsync().Result ?? new UserSession();

        Themes = new ObservableCollection<CustomAppTheme>((CustomAppTheme[])Enum.GetValues(typeof(CustomAppTheme)));
        if (userSession.SelectedAppTheme.HasValue)
        {
            SelectedTheme = userSession.SelectedAppTheme.Value;
        }
    }
    
    partial void OnSelectedThemeChanged(CustomAppTheme value)
    {
        _themeService.SetTheme(value);
    }

    private async Task OnNextAsync()
    {
        var userSession = _userService.GetUserSessionAsync().Result ?? new UserSession();
        userSession.SelectedAppTheme = SelectedTheme;
        await _userService.SaveUserSessionAsync(userSession);
        
        if (userSession.CurrentLoginMode == LoginMode.Registered)
        {
            await _navigation.PushAsync(new SyncSelectionPage(_userService, _walletService, _currencyService, _navigationService, _mapper, _dialogService));
        }
        else
        {
            await _navigation.PushAsync(new InitialWalletCreationPage(_walletService, _currencyService, _navigationService, _mapper, _dialogService));
        }
    }
}