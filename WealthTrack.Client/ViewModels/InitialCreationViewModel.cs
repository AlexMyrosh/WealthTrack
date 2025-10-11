using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.BusinessModels.Currency;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.ViewModels;

public class InitialCreationViewModel : BaseViewModel
{
    private readonly IWalletService _walletService;
    private readonly ICurrencyService _currencyService;
    private readonly IAuthService _authService;

    private ObservableCollection<CurrencyDetailsBusinessModel> _currencies = new();
    public ObservableCollection<CurrencyDetailsBusinessModel> Currencies
    {
        get => _currencies;
        set => SetProperty(ref _currencies, value);
    }

    private CurrencyDetailsBusinessModel? _selectedCurrency;
    public CurrencyDetailsBusinessModel? SelectedCurrency
    {
        get => _selectedCurrency;
        set => SetProperty(ref _selectedCurrency, value);
    }

    private string _walletName = string.Empty;
    public string WalletName
    {
        get => _walletName;
        set => SetProperty(ref _walletName, value);
    }

    private decimal _initialBalance;
    public decimal InitialBalance
    {
        get => _initialBalance;
        set => SetProperty(ref _initialBalance, value);
    }

    private string _stepTitle = string.Empty;
    public string StepTitle
    {
        get => _stepTitle;
        set => SetProperty(ref _stepTitle, value);
    }

    private string _stepDescription = string.Empty;
    public string StepDescription
    {
        get => _stepDescription;
        set => SetProperty(ref _stepDescription, value);
    }
    
    public ICommand CreateWalletCommand { get; }
    public ICommand SkipWalletCommand { get; }

    public InitialCreationViewModel(IWalletService walletService, ICurrencyService currencyService, IAuthService authService)
    {
        _walletService = walletService;
        _currencyService = currencyService;
        _authService = authService;

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
        var session = await _authService.GetUserSessionAsync();
        if (session is not null)
        {
            session.IsIntroductionCompleted = true;
            await _authService.SaveUserSessionAsync(session);
        }
        
        await Shell.Current.GoToAsync("//TransactionsPage");
    }
}
