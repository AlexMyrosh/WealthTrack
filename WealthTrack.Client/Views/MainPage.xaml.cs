using WealthTrack.Client.ViewModels;

namespace WealthTrack.Client.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}