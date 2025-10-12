using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views.Account;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.ViewModels.Account;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IWalletService _walletService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public INavigation Navigation { get; set; } = null!;
    public ICommand RegularLoginCommand { get; }
    public ICommand ResetPasswordCommand { get; }
    public ICommand GoogleLoginCommand { get; }
    public ICommand AppleLoginCommand { get; }
    public ICommand MicrosoftLoginCommand { get; }
    public ICommand NavigateToAccountCreationPageCommand { get; }
    public ICommand ContinueWithoutAccountCommand { get; }
    
    [ObservableProperty] private string _email = string.Empty;
    
    [ObservableProperty] private string _password = string.Empty;
    
    public LoginViewModel(IAuthService authService, IWalletService walletService, INavigationService navigationService, IDialogService dialogService)
    {
        _authService  = authService;
        _walletService = walletService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        
        RegularLoginCommand = new AsyncRelayCommand(RegularLoginAsync);
        ResetPasswordCommand = new AsyncRelayCommand(ResetPasswordAsync);
        GoogleLoginCommand = new AsyncRelayCommand(GoogleLoginAsync);
        AppleLoginCommand = new AsyncRelayCommand(AppleLoginAsync);
        MicrosoftLoginCommand = new AsyncRelayCommand(MicrosoftLoginAsync);
        NavigateToAccountCreationPageCommand = new AsyncRelayCommand(GoToAccountCreationPage);
        ContinueWithoutAccountCommand = new AsyncRelayCommand(ContinueWithoutAccountAsync);
    }
    
    private async Task GoToAccountCreationPage()
    {
        await Navigation.PushAsync(new AccountCreationPage(_authService, _dialogService, _navigationService));
    }
    
    private async Task RegularLoginAsync()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            await _dialogService.ShowAlertAsync("Error", "Please enter both email and password", "OK");
            return;
        }
        
        if (await _authService.LoginAsync(Email, Password))
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await _dialogService.ShowAlertAsync("Error", "Login failed", "OK");
        }
    }
    
    private async Task ResetPasswordAsync()
    {
        await Navigation.PushAsync(new ForgotPasswordPage(_authService, _navigationService, _dialogService));
    }

    private async Task GoogleLoginAsync()
    {
        if (await _authService.LoginWithGoogleAsync())
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await _dialogService.ShowAlertAsync("Error", "Google login failed", "OK");
        }
    }

    private async Task AppleLoginAsync()
    {
        if (await _authService.LoginWithAppleAsync())
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await _dialogService.ShowAlertAsync("Error", "Apple login failed", "OK");
        }
    }
    
    private async Task MicrosoftLoginAsync()
    {
        if (await _authService.LoginWithMicrosoftAsync())
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await _dialogService.ShowAlertAsync("Error", "Microsoft login failed", "OK");
        }
    }
    
    private async Task ContinueWithoutAccountAsync()
    {
        await _authService.ContinueWithoutAccountAsync();
        await MoveToNextStepAsync();
    }
    
    private async Task MoveToNextStepAsync()
    {
        var session = await _authService.GetUserSessionAsync() ?? new UserSession
        {
            CurrentLoginMode = LoginMode.Guest
        };

        if (!session.IsIntroductionCompleted || !session.AccountCurrencyId.HasValue || !session.SelectedAppTheme.HasValue)
        {
            await _navigationService.GoToAsync("//InitialAccountConfigurationPage");
        }
        else if (!await _walletService.AnyAsync())
        {
            await _navigationService.GoToAsync("//InitialCreationPage");
        }
        else
        {
            await _navigationService.GoToAsync("//TransactionsPage");
        }
    }
}