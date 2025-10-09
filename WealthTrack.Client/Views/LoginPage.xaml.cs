using WealthTrack.Client.Services;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Views;

public partial class LoginPage : ContentPage
{
    private readonly IAuthService _authService;
    
    public LoginPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
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
            await Shell.Current.GoToAsync("//MainPage");
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
            await Shell.Current.GoToAsync("//MainPage");
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
            await Shell.Current.GoToAsync("//MainPage");
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
            await Shell.Current.GoToAsync("//MainPage");
        }
        else
        {
            await DisplayAlert("Error", "Microsoft login failed", "OK");
        }
    }
    
    private async void OnContinueWithoutAccountClicked(object sender, EventArgs e)
    {
        await _authService.ContinueWithoutAccountAsync();
        await Shell.Current.GoToAsync("//MainPage");
    }
}