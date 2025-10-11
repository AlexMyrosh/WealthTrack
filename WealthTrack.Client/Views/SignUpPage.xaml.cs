using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Views;

public partial class SignUpPage : ContentPage
{
    private readonly IAuthService _authService;
    
    public SignUpPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        var firstName = FirstNameEntry.Text.Trim();
        var lastName = LastNameEntry.Text.Trim();
        var email = EmailEntry.Text.Trim();
        var password = PasswordEntry.Text;

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please fill in all required fields.", "OK");
            return;
        }
        
        if (await _authService.SignUpAsync(firstName, lastName, email, password))
        {
            await Shell.Current.GoToAsync("//TransactionsPage");
        }
        else
        {
            await DisplayAlert("Error", "Sign up failed", "OK");
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}