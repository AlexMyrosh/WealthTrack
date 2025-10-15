using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Models.Dto;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

public partial class CurrencySelectionViewModel : ObservableObject
{
    private readonly IUserService _userService;
    private readonly ICurrencyService _currencyService;
    private readonly IWalletService _walletService;
    private readonly IThemeService _themeService;
    private readonly IMapper _mapper;
    
    public INavigation Navigation { get; set; } = null!;
    
    [ObservableProperty] private ObservableCollection<CurrencyDto> _currencies = new();
    [ObservableProperty] private CurrencyDto? _selectedCurrency;

    public ICommand NextCommand { get; }

    public CurrencySelectionViewModel(IUserService userService, ICurrencyService currencyService, IWalletService walletService, IThemeService themeService, IMapper mapper)
    {
        _userService = userService;
        _currencyService = currencyService;
        _walletService = walletService;
        _themeService = themeService;
        _mapper = mapper;

        NextCommand = new AsyncRelayCommand(OnNextAsync);

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var currencies = await _currencyService.GetAllAsync();
        var userSession = await _userService.GetUserSessionAsync();
        if (userSession.AccountCurrencyId != null)
        {
            var selectedCurrency = currencies.FirstOrDefault(c => c.Id == userSession.AccountCurrencyId.Value);
            SelectedCurrency = _mapper.Map<CurrencyDto>(selectedCurrency);
            return;
        }
        
        // In the future, it will be needed to search for default currency by user's country
        Currencies = new ObservableCollection<CurrencyDto>(_mapper.Map<List<CurrencyDto>>(currencies));
        SelectedCurrency = Currencies.FirstOrDefault(c => c.Code == "USD") ?? Currencies.FirstOrDefault();
    }

    private async Task OnNextAsync()
    {
        var userSession = await _userService.GetUserSessionAsync();
        if (SelectedCurrency != null)
        {
            userSession.AccountCurrencyId = SelectedCurrency.Id;
            await _userService.SaveUserSessionAsync(userSession);
        }
        
        await Navigation.PushAsync(new ThemeSelectionPage(_userService, _themeService, _walletService, _currencyService));
    }
}