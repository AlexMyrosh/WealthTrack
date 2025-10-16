using AutoMapper;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

public partial class InitialWalletCreationPage : ContentPage
{
    public InitialWalletCreationPage(IWalletService walletService, ICurrencyService currencyService, INavigationService navigationService, IMapper mapper, IDialogService dialogService)
    {
        InitializeComponent();
        BindingContext = new InitialWalletCreationViewModel(walletService, navigationService, currencyService, mapper, dialogService);
    }
}