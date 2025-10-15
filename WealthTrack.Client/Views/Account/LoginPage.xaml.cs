using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Account;

namespace WealthTrack.Client.Views.Account;

public partial class LoginPage
{
    public LoginPage(IAuthService authService, IUserService userService, INavigationService navigationService, IDialogService dialogService)
    {
        InitializeComponent();
        var viewModel = new LoginViewModel(authService, userService, navigationService, dialogService);
        BindingContext = viewModel;
        viewModel.Navigation = Navigation;
    }
}