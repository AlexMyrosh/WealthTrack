using System.ComponentModel;
using System.Windows.Input;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;
using CommunityToolkit.Mvvm.Input;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IAuthService _authService;
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isGuest;
    public bool IsGuest
    {
        get => _isGuest;
        set
        {
            if (_isGuest != value)
            {
                _isGuest = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGuest)));
            }
        }
    }

    public ICommand LogoutCommand { get; }
    public ICommand LoginCommand { get; }

    public MainViewModel(IAuthService authService)
    {
        _authService = authService;

        LogoutCommand = new AsyncRelayCommand(OnLogout);
        LoginCommand = new AsyncRelayCommand(OnLogin);
        
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var session = await _authService.GetUserSessionAsync();
        IsGuest = session == null || session.CurrentLoginMode == LoginMode.Guest;
    }

    private async Task OnLogout()
    {
        await _authService.LogoutAsync();
        IsGuest = true;
    }

    private async Task OnLogin()
    {
        await Shell.Current.GoToAsync("//LoginPage");
    }
}