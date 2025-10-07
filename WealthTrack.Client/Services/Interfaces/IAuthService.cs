namespace WealthTrack.Client.Services.Interfaces;

public interface IAuthService
{
    public Task<bool> LoginAsync(string email, string password);
    
    public Task<bool> LoginWithGoogleAsync();
    
    public Task<bool> LoginWithAppleAsync();
    
    public Task LogoutAsync();

    public Task<string?> GetTokenAsync();
}
