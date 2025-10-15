using WealthTrack.Client.Models;

namespace WealthTrack.Client.Services.Interfaces;

public interface IUserService
{
    public Task<UserSession?> GetUserSessionAsync();

    public Task SaveUserSessionAsync(UserSession session);
}