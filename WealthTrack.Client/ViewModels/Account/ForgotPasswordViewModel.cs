using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.Views.Account;

namespace WealthTrack.Client.ViewModels.Account;

public partial class ForgotPasswordViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly INavigation _navigation;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _code = string.Empty;
    [ObservableProperty] private bool _isEmailStepEnabled = true;
    [ObservableProperty] private bool _isCodeStepVisible;
    [ObservableProperty] private bool _isSendCodeButtonVisible = true;
    [ObservableProperty] private bool _isResendCodeButtonVisible;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ResendButtonBackground))] private bool _canResendCode;
    [ObservableProperty] private string _resendCodeButtonText = "Resend Code";
    
    public Color ResendButtonBackground => CanResendCode 
        ? (Color)Application.Current!.Resources["ButtonBackground"] 
        : (Color)Application.Current!.Resources["ButtonDisabledBackground"];
    
    public ICommand SendCodeCommand { get; }
    public ICommand VerifyCodeCommand { get; }
    public ICommand ResendCodeCommand { get; }

    public ForgotPasswordViewModel(IAuthService authService, INavigation navigation, INavigationService navigationService, IDialogService dialogService)
    {
        _authService = authService;
        _navigationService  = navigationService;
        _dialogService = dialogService;
        _navigation = navigation;

        SendCodeCommand = new AsyncRelayCommand(SendCodeAsync);
        VerifyCodeCommand = new AsyncRelayCommand(VerifyCodeAsync);
        ResendCodeCommand = new AsyncRelayCommand(ResendCodeAsync);
    }

    private async Task SendCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            await _dialogService.ShowAlertAsync("Error", "Enter is required", "OK");
            return;
        }
        
        IsEmailStepEnabled = false;
        IsCodeStepVisible = true;
        StartResendTimer();

        await _authService.RequestPasswordResetAsync(Email);
    }

    private async Task ResendCodeAsync()
    {
        if (CanResendCode)
        {
            await _authService.RequestPasswordResetAsync(Email);
            StartResendTimer();
        }
    }

    private async Task VerifyCodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Code) || Code.Length != 6)
        {
            await _dialogService.ShowAlertAsync("Error", "Please enter a valid 6-digit code.", "OK");
            return;
        }

        var resetToken = await _authService.VerifyResetCodeAsync(Email, Code);
        if (resetToken == null)
        {
            await _dialogService.ShowAlertAsync("Error", "Code is invalid or expired", "OK");
            return;
        }
        
        await _navigation.PushAsync(new ResetPasswordPage(_authService, _navigationService, _dialogService, resetToken));
    }
    
    private void StartResendTimer()
    {
        IsResendCodeButtonVisible = true;
        IsSendCodeButtonVisible = false;
        CanResendCode = false;
        var seconds = 60;
        ResendCodeButtonText = $"Resend Code ({seconds})";
        Application.Current!.Dispatcher.StartTimer(TimeSpan.FromSeconds(1), () =>
        {
            seconds--;
            ResendCodeButtonText = seconds > 0 ? $"Resend Code ({seconds})" : "Resend Code";
            CanResendCode = seconds <= 0;
            return seconds > 0;
        });
    }
}