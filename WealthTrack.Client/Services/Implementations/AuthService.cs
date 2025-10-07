using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;

namespace WealthTrack.Client.Services.Implementations;

public class AuthService(HttpClient http, OAuthSettings settings) : IAuthService
{
    private const string AuthTokenKey = "auth_token";

    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var token = doc.RootElement.GetProperty("token").GetString();
        if (string.IsNullOrEmpty(token))
            return false;

        await SecureStorage.SetAsync(AuthTokenKey, token);
        return true;
    }

    public async Task<bool> LoginWithGoogleAsync()
    {
        var platform = GetPlatform();
        var google = platform switch
        {
            "iOS" => settings.Google.iOS,
            "Android" => settings.Google.Android,
            "Windows" => settings.Google.Desktop,
            "MacCatalyst" => settings.Google.Desktop,
            _ => throw new NotSupportedException($"Platform {platform} is not supported")
        };

        var authUrl = new Uri(
            $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={google.ClientId}" +
            $"&redirect_uri={google.RedirectUri}" +
            $"&response_type=code" +
            $"&scope=openid%20email%20profile"
        );

        try
        {
            var result = await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = authUrl,
                    CallbackUrl = new Uri(google.RedirectUri)
                });

            if (!result.Properties.TryGetValue("code", out var code))
                return false;

            // Обмен кода на JWT через Sync Server
            var response = await http.PostAsJsonAsync("api/auth/oauth/google", new
            {
                Code = code,
                Platform = platform
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var token = doc.RootElement.GetProperty("token").GetString();
            if (string.IsNullOrEmpty(token))
                return false;

            await SecureStorage.SetAsync(AuthTokenKey, token);
            return true;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    public async Task<bool> LoginWithAppleAsync()
    {
        // Just a mock at the moment
        await Task.Delay(500);
        await SecureStorage.SetAsync(AuthTokenKey, "FAKE_APPLE_JWT");
        return true;
    }

    public Task LogoutAsync()
    {
        SecureStorage.Remove(AuthTokenKey);
        return Task.CompletedTask;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(AuthTokenKey);
    }

    private static string GetPlatform()
    {
        return DeviceInfo.Platform.ToString();
    }
}