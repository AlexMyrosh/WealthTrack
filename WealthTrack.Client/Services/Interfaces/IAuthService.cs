using WealthTrack.Client.Models;

namespace WealthTrack.Client.Services.Interfaces;

public interface IAuthService
{
    public Task<bool> LoginAsync(string email, string password);
    
    public Task<bool> SignUpAsync(string firstName, string lastName, string email, string password);
    
    public Task<bool> LoginWithGoogleAsync();
    
    public Task<bool> LoginWithAppleAsync();
    
    public Task<bool> LoginWithMicrosoftAsync();
    
    public Task LogoutAsync();

    public Task ContinueWithoutAccountAsync();

    public Task<UserSession?> GetUserSessionAsync();

    public Task SaveUserSessionAsync(UserSession session);
}
