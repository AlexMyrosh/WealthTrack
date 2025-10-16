using System.Collections.ObjectModel;
using System.Windows.Input;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.BusinessModels.Wallet;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Models.Dto;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views.Transaction;

namespace WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

    public partial class InitialWalletCreationViewModel : ObservableObject
    {
        private readonly IWalletService _walletService;
        private readonly INavigationService _navigationService;
        private readonly ICurrencyService _currencyService;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;

        [ObservableProperty] private ObservableCollection<WalletDto> _wallets = [];
        [ObservableProperty] private ObservableCollection<CurrencyDto> _currencies = [];
        [ObservableProperty] private CurrencyDto? _selectedCurrency;
        [ObservableProperty] private string _newWalletName = string.Empty;
        [ObservableProperty] private decimal _newWalletBalance;
        [ObservableProperty] private bool _isPartOfGeneralBalance;

        public ICommand AddWalletCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand DeleteWalletCommand { get; }

        public string CompleteButtonText => Wallets.Any() ? "Complete configuration" : "Create later";

        public InitialWalletCreationViewModel(IWalletService walletService, INavigationService navigationService, ICurrencyService currencyService, IMapper mapper, IDialogService dialogService)
        {
            _walletService = walletService;
            _navigationService = navigationService;
            _currencyService = currencyService;
            _mapper = mapper;
            _dialogService = dialogService;

            AddWalletCommand = new AsyncRelayCommand(AddWalletAsync);
            CompleteCommand = new AsyncRelayCommand(CompleteAsync);
            DeleteWalletCommand = new AsyncRelayCommand<Guid>(DeleteWalletAsync);
            
            _ = LoadDataAsync();
        }
        
        private async Task LoadDataAsync()
        {
            var currencies = await _currencyService.GetAllAsync();
            var wallets = await _walletService.GetAllAsync();
            Wallets = new ObservableCollection<WalletDto>(_mapper.Map<List<WalletDto>>(wallets));
            Currencies = new ObservableCollection<CurrencyDto>(_mapper.Map<List<CurrencyDto>>(currencies));
            if (Currencies.Any())
            {
                // In the future, it will be needed to search for default currency by user's country
                SelectedCurrency = (Currencies.FirstOrDefault(c => c.Code == "USD") ?? Currencies.FirstOrDefault())!;
            }
        }
        
        private async Task AddWalletAsync()
        {
            if (string.IsNullOrWhiteSpace(NewWalletName))
            {
                await _dialogService.ShowAlertAsync("Error", "Name is required", "OK");
                return;
            }
            if (SelectedCurrency is null)
            {
                await _dialogService.ShowAlertAsync("Error", "Currency is required", "OK");
                return;
            }
            
            var wallet = new WalletUpsertBusinessModel
            {
                Name = NewWalletName,
                Balance = NewWalletBalance,
                IsPartOfGeneralBalance = IsPartOfGeneralBalance,
                CurrencyId = SelectedCurrency.Id
            };

            var walletId = await _walletService.CreateAsync(wallet);
            Wallets.Add(new WalletDto
            {
                Id = walletId,
                Name = NewWalletName,
                Balance = NewWalletBalance,
                IsPartOfGeneralBalance = IsPartOfGeneralBalance,
                Currency = SelectedCurrency
            });
            NewWalletName = string.Empty;
            NewWalletBalance = 0;

            OnPropertyChanged(nameof(CompleteButtonText));
        }
        
        private async Task DeleteWalletAsync(Guid id)
        {
            var wallet = Wallets.First(w => w.Id == id);
            Wallets.Remove(wallet);
            await _walletService.HardDeleteAsync(id);
        }

        private async Task CompleteAsync()
        {
            await _navigationService.GoToAsync($"//{nameof(TransactionsPage)}");
        }
    }
