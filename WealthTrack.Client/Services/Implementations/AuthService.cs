using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.Services.Implementations;

public class AuthService(HttpClient http, OAuthSettings settings) : IAuthService
{
    private const string StorageKey = "user-session";

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await http.PostAsJsonAsync("api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json);
            if (session == null)
            {
                return false;
            }

            session.CurrentLoginMode = LoginMode.Registered;
            await SaveUserSessionAsync(session);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SignUpAsync(string firstName, string lastName, string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/register", new
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email, 
            Password = password
        });
        
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var json = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<UserSession>(json);
        if (session == null)
        {
            return false;
        }

        session.CurrentLoginMode = LoginMode.Registered;
        await SaveUserSessionAsync(session);
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

        // PKCE setup
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        var authUrl = new Uri(
            $"https://accounts.google.com/o/oauth2/v2/auth?" +
            $"client_id={Uri.EscapeDataString(google.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(google.RedirectUri)}" +
            $"&response_type=code" +
            $"&scope=openid%20email%20profile" +
            $"&code_challenge={codeChallenge}" +
            $"&code_challenge_method=S256"
        );

        try
        {
            var result = await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
            {
                Url = authUrl,
                CallbackUrl = new Uri(google.RedirectUri)
            });

            if (!result.Properties.TryGetValue("code", out var code))
                return false;
            
            var response = await http.PostAsJsonAsync("api/auth/oauth/google", new
            {
                Code = code,
                ClientId = google.ClientId,
                RedirectUri = google.RedirectUri,
                CodeVerifier = codeVerifier
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json);
            if (session == null)
            {
                return false;
            }

            session.CurrentLoginMode = LoginMode.Registered;
            await SaveUserSessionAsync(session);
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
        var session = new UserSession
        {
            CurrentLoginMode = LoginMode.Registered,
            Token = "FAKE_APPLE_JWT"
        };

        await SaveUserSessionAsync(session);
        return true;
    }

    public async Task<bool> LoginWithMicrosoftAsync()
    {
        var pca = PublicClientApplicationBuilder
            .Create(settings.Microsoft.ClientId)
            .WithRedirectUri(settings.Microsoft.RedirectUri)
            .Build();
        
        AuthenticationResult result;
        try
        {
            var accounts = await pca.GetAccountsAsync();
            result =  await pca.AcquireTokenSilent(settings.Microsoft.Scopes, accounts.FirstOrDefault()).ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            result = await pca.AcquireTokenInteractive(settings.Microsoft.Scopes).ExecuteAsync();
        }
        
        var token = result.AccessToken;
        var response = await http.PostAsJsonAsync("api/auth/oauth/microsoft", new
        {
            Token = token
        });

        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        
        var json = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<UserSession>(json);
        if (session == null)
        {
            return false;
        }

        session.CurrentLoginMode = LoginMode.Registered;
        await SaveUserSessionAsync(session);
        return true;
    }
    
    public Task LogoutAsync()
    {
        SecureStorage.Remove(StorageKey);
        return Task.CompletedTask;
    }

    public async Task ContinueWithoutAccountAsync()
    {
        var session = new UserSession
        {
            CurrentLoginMode = LoginMode.Guest,
        };

        await SaveUserSessionAsync(session);
    }
    
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

    private static string GetPlatform()
    {
        return DeviceInfo.Platform.ToString();
    }
    
    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(codeVerifier));
        return Base64UrlEncode(bytes);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}