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
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter both email and password.", "OK");
            return;
        }
        
        await DisplayAlert("Login", $"Email: {email}\nPassword: {password}", "OK");
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnGoogleLoginClicked(object sender, EventArgs e)
    {
        if (await _authService.LoginWithGoogleAsync())
            await Shell.Current.GoToAsync("//MainPage");
        else
            await DisplayAlert("Error", "Google login failed", "OK");
    }

    private async void OnAppleLoginClicked(object sender, EventArgs e)
    {
        if (await _authService.LoginWithAppleAsync())
            await Shell.Current.GoToAsync("//MainPage");
        else
            await DisplayAlert("Error", "Apple login failed", "OK");
    }
}