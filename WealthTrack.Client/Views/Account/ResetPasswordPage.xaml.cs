using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Account;

namespace WealthTrack.Client.Views.Account;

public partial class ResetPasswordPage : ContentPage
{
    public ResetPasswordPage(IAuthService authService, INavigationService navigationService, IDialogService dialogService, string token)
    {
        InitializeComponent();
        BindingContext = new ResetPasswordViewModel(authService, navigationService, dialogService, token);
    }
}