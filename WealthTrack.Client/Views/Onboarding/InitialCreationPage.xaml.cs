using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Onboarding;

namespace WealthTrack.Client.Views.Onboarding;

public partial class InitialCreationPage : ContentPage
{
    public InitialCreationPage(IWalletService walletService, ICurrencyService currencyService, IAuthService authService)
    {
        InitializeComponent();
        BindingContext = new InitialCreationViewModel(walletService, currencyService, authService);
    }
}