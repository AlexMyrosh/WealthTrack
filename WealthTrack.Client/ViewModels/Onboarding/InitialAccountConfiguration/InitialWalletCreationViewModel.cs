using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

public partial class InitialWalletCreationViewModel : ObservableObject
{
    private readonly IWalletService _walletService;
    private readonly ICurrencyService _currencyService;
    private readonly IUserService _userService;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private ObservableCollection<CurrencyDetailsBusinessModel> _currencies = [];
    [ObservableProperty] private CurrencyDetailsBusinessModel? _selectedCurrency;
    [ObservableProperty] private string _walletName = string.Empty;
    [ObservableProperty] private decimal _initialBalance;
    [ObservableProperty] private string _stepTitle = string.Empty;
    [ObservableProperty] private string _stepDescription = string.Empty;
    public ICommand CreateWalletCommand { get; }
    public ICommand SkipWalletCommand { get; }

    public InitialWalletCreationViewModel(IWalletService walletService, ICurrencyService currencyService, IUserService userService)
    {
        _walletService = walletService;
        _currencyService = currencyService;
        _userService = userService;

        CreateWalletCommand = new AsyncRelayCommand(CreateWalletAsync);
        SkipWalletCommand = new AsyncRelayCommand(SkipWalletAsync);

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            await LoadCurrenciesAsync();
            await CheckExistingDataAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadCurrenciesAsync()
    {
        var currencies = await _currencyService.GetAllAsync();
        Currencies.Clear();
        foreach (var currency in currencies)
        {
            Currencies.Add(currency);
        }

        // Set default currency (USD if available)
        SelectedCurrency = Currencies.FirstOrDefault(c => c.Code == "USD") ?? Currencies.FirstOrDefault();
    }

    private async Task CheckExistingDataAsync()
    {
        var wallets = await _walletService.GetAllAsync();
        if (wallets.Count == 0)
        {
            StepTitle = "Create Your First Wallet";
            StepDescription = "Now let's create a wallet to start tracking your money.";
        }
        else
        {
            await NavigateToMainPageAsync();
        }
    }
    
    private async Task CreateWalletAsync()
    {
        if (SelectedCurrency == null || string.IsNullOrWhiteSpace(WalletName))
        {
            return;
        }

        IsBusy = true;
        try
        {
            var request = new WalletUpsertBusinessModel
            {
                Name = WalletName,
                CurrencyId = SelectedCurrency.Id,
                Balance = InitialBalance,
                IsPartOfGeneralBalance = true
            };

            await _walletService.CreateAsync(request);
            await NavigateToMainPageAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SkipWalletAsync()
    {
        await NavigateToMainPageAsync();
    }

    private async Task NavigateToMainPageAsync()
    {
        var session = await _userService.GetUserSessionAsync();
        if (session is not null)
        {
            session.IsIntroductionCompleted = true;
            await _userService.SaveUserSessionAsync(session);
        }
        
        await Shell.Current.GoToAsync("//TransactionsPage");
    }
}
