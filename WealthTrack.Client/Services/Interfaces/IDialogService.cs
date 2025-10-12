namespace WealthTrack.Client.Services.Interfaces;

public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel);
    
    Task<bool> ShowConfirmationAsync(string title, string message, string accept, string cancel);

    public Task<string> ShowDialogAsync(string title, string message, string accept, string cancel, string placeholder, int maxLength, Keyboard keyboard);
}