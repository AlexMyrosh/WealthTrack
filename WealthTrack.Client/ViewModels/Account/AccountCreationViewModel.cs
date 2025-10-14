using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.ViewModels.Account;

public partial class AccountCreationViewModel : ObservableObject
{
    public INavigation Navigation { get; set; } = null!;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    
    public ICommand CreateAccountCommand { get; }
    public ICommand NavigateToLoginPageCommand { get; }
    
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    
    public AccountCreationViewModel(IAuthService authService, IDialogService dialogService, INavigationService navigationService)
    {
        _authService = authService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        
        CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync);
        NavigateToLoginPageCommand = new AsyncRelayCommand(GoToLoginPageAsync);
    }
    
    private async Task CreateAccountAsync()
    {
        if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
        {
            await _dialogService.ShowAlertAsync("Error", "Please fill in all required fields", "OK");
            return;
        }
        
        FullName = FullName.Trim();
        Email = Email.Trim();
        
        if (await _authService.SignUpAsync(FullName, Email, Password))
        {
            await _navigationService.GoToAsync("//InitialAccountConfigurationPage");
        }
        else
        {
            await _dialogService.ShowAlertAsync("Error", "Account creation failed", "OK");
        }
    }

    private async Task GoToLoginPageAsync()
    {
        await Navigation.PopAsync();
    }
}