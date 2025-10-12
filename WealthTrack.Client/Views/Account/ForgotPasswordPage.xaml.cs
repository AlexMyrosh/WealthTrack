using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels.Account;

namespace WealthTrack.Client.Views.Account;

public partial class ForgotPasswordPage : ContentPage
{
    public ForgotPasswordPage(IAuthService authService, INavigationService navigationService, IDialogService dialogService)
    {
        InitializeComponent();
        BindingContext = new ForgotPasswordViewModel(authService, Navigation, navigationService, dialogService);
    }
}