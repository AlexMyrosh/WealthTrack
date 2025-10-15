using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.ViewModels.Account;

public partial class ResetPasswordViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly string _token;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    public ICommand ResetPasswordCommand { get; }

    public ResetPasswordViewModel(IAuthService authService, INavigationService navigationService, IDialogService dialogService, string token)
    {
        _authService = authService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _token = token;
        
        ResetPasswordCommand = new AsyncRelayCommand(ResetAsync);
    }

    private async Task ResetAsync()
    {
        if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
        {
            await _dialogService.ShowAlertAsync("Error", "Please enter both password and confirm password", "OK");
            return;
        }
        
        if (Password != ConfirmPassword)
        {
            await _dialogService.ShowAlertAsync("Error", "Passwords do not match", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var result = await _authService.ResetPasswordAsync(_token, Password);
            if (result)
            {
                await _dialogService.ShowAlertAsync("Success", "Password successfully updated", "OK");
                await _navigationService.GoToAsync("//LoginPage");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Error", "Failed to reset password", "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }

    }
}