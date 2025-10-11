using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels;

namespace WealthTrack.Client.Views;

public partial class InitialCreationPage : ContentPage
{
    public InitialCreationPage(IWalletService walletService, ICurrencyService currencyService, IAuthService authService)
    {
        InitializeComponent();
        BindingContext = new InitialCreationViewModel(walletService, currencyService, authService);
    }
}