using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Onboarding.InitialAccountConfiguration;

namespace WealthTrack.Client.Views.Onboarding.InitialAccountConfiguration;

public partial class InitialWalletCreationPage : ContentPage
{
    public InitialWalletCreationPage(IWalletService walletService, ICurrencyService currencyService, IUserService userService)
    {
        InitializeComponent();
        BindingContext = new InitialWalletCreationViewModel(walletService, currencyService, userService);
    }
}