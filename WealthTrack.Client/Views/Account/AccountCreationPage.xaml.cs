using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Account;

namespace WealthTrack.Client.Views.Account;

public partial class AccountCreationPage
{
    public AccountCreationPage(IAuthService authService, IDialogService dialogService, INavigationService navigationService)
    {
        InitializeComponent();
        BindingContext = new AccountCreationViewModel(authService, dialogService, navigationService, Navigation);
    }
}