using System.Windows.Input;

namespace WealthTrack.Client.ViewModels;

public class MainViewModel : BaseViewModel
{
    public ICommand LogoutCommand { get; }

    public MainViewModel()
    {
        LogoutCommand = new Command(OnLogout);
    }

    private async void OnLogout()
    {
        // Удаляем токен или данные пользователя
        await SecureStorage.SetAsync("auth_token", string.Empty);

        // Переходим на страницу логина, очищая стек
        await Shell.Current.GoToAsync("//LoginPage");
    }
}