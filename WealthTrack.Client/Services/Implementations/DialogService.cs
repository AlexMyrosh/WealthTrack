using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Services.Implementations;

public class DialogService : IDialogService
{
    public async Task ShowAlertAsync(string title, string message, string cancel)
    {
        await Application.Current.MainPage.DisplayAlert(title, message, cancel);
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel)
    {
        return await Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
    }
    
    public async Task<string> ShowDialogAsync(string title, string message, string accept, string cancel, string placeholder, int maxLength, Keyboard keyboard)
    {
        return await Application.Current.MainPage.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, keyboard);
    }
}