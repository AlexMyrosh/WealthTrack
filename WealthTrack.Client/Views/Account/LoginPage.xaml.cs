using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Account;

namespace WealthTrack.Client.Views.Account;

public partial class LoginPage
{
    public LoginPage(IAuthService authService, IWalletService walletService, INavigationService navigationService, IDialogService dialogService)
    {
        InitializeComponent();
        var viewModel = new LoginViewModel(authService, walletService, navigationService, dialogService);
        BindingContext = viewModel;
        viewModel.Navigation = Navigation;
    }
}