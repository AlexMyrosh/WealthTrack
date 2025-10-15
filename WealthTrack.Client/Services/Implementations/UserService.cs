using System.Text.Json;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Services.Implementations;

public class UserService : IUserService
{
    private const string StorageKey = "user-session";
    
    public async Task<UserSession?> GetUserSessionAsync()
    {
        var json = await SecureStorage.GetAsync(StorageKey);
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserSession>(json);
    }
    
    public async Task SaveUserSessionAsync(UserSession session)
    {
        var json = JsonSerializer.Serialize(session);
        await SecureStorage.SetAsync(StorageKey, json);
    }
}