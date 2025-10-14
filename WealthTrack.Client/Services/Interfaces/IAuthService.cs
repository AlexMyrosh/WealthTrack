using WealthTrack.Client.Models;

namespace WealthTrack.Client.Services.Interfaces;

public interface IAuthService
{
    public Task<bool> LoginAsync(string email, string password);
    
    public Task<bool> SignUpAsync(string fullName, string email, string password);

    public Task<bool> RequestPasswordResetAsync(string email);

    public Task<string?> VerifyResetCodeAsync(string email, string code);

    public Task<bool> ResetPasswordAsync(string token, string newPassword);
    
    public Task<bool> LoginWithGoogleAsync();
    
    public Task<bool> LoginWithAppleAsync();
    
    public Task<bool> LoginWithMicrosoftAsync();
    
    public Task LogoutAsync();

    public Task ContinueWithoutAccountAsync();

    public Task<UserSession?> GetUserSessionAsync();

    public Task SaveUserSessionAsync(UserSession session);
}
