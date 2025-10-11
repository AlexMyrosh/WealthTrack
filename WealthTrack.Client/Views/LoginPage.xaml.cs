using WealthTrack.Business.Services.Interfaces;
using WealthTrack.Client.Services;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Views;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    private readonly IWalletService _walletService;
    
    public LoginPage(IAuthService authService, IWalletService walletService)
    {
        InitializeComponent();
        _authService = authService;
        _walletService = walletService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry?.Text?.Trim() ?? string.Empty;
        var password = PasswordEntry?.Text ?? string.Empty;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }
        
        if (await _authService.LoginAsync(email, password))
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await DisplayAlert("Error", "Login failed", "OK");
        }
    }
    
    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage(_authService));
    }

    private async void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        if (await _authService.LoginWithGoogleAsync())
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await DisplayAlert("Error", "Google login failed", "OK");
        }
    }

    private async void OnAppleLoginClicked(object sender, EventArgs e)
    {
        if (await _authService.LoginWithAppleAsync())
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await DisplayAlert("Error", "Apple login failed", "OK");
        }
    }
    
    private async void OnMicrosoftLoginClicked(object sender, EventArgs e)
    {
        if (await _authService.LoginWithMicrosoftAsync())
        {
            await MoveToNextStepAsync();
        }
        else
        {
            await DisplayAlert("Error", "Microsoft login failed", "OK");
        }
    }
    
    private async void OnContinueWithoutAccountClicked(object sender, EventArgs e)
    {
        await _authService.ContinueWithoutAccountAsync();
        await MoveToNextStepAsync();
    }

    private async Task MoveToNextStepAsync()
    {
        var wallets = await _walletService.GetAllAsync();
        if (wallets.Count == 0)
        {
            await Shell.Current.GoToAsync("//InitialCreationPage");
        }
        else
        {
            var session = await _authService.GetUserSessionAsync();
            if (session is not null)
            {
                session.IsIntroductionCompleted = true;
                await _authService.SaveUserSessionAsync(session);
            }
        
            await Shell.Current.GoToAsync("//TransactionsPage");
        }
    }
}