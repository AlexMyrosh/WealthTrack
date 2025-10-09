using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Client.ViewModels;

namespace WealthTrack.Client.Views;

public partial class MainPage : ContentPage
{
    public MainPage(IAuthService authService)
    {
        InitializeComponent();
        BindingContext = new MainViewModel(authService);
    }
}