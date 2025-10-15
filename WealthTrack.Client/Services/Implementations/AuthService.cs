using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.Identity.Client;
using WealthTrack.Client.Models;
using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.Services.Implementations;

public class AuthService(HttpClient httpClient, OAuthSettings settings, IUserService userService) : IAuthService
{
    private const string StorageKey = "user-session";

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", new
            {
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json, JsonSerializerOptions.Web);
            if (session == null)
            {
                return false;
            }

            session.CurrentLoginMode = LoginMode.Registered;
            await userService.SaveUserSessionAsync(session);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SignUpAsync(string fullName, string email, string password)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/register", new
            {
                Fullname = fullName,
                Email = email,
                Password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json, JsonSerializerOptions.Web);
            if (session == null)
            {
                return false;
            }

            session.CurrentLoginMode = LoginMode.Registered;
            await userService.SaveUserSessionAsync(session);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/request-password-reset", new { email });
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> VerifyResetCodeAsync(string email, string code)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/verify-reset-code", new { email, code });
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("token").GetString();
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/reset-password", new { token, newPassword });
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginWithGoogleAsync()
    {
        try
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

            var result = await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
            {
                Url = authUrl,
                CallbackUrl = new Uri(google.RedirectUri)
            });

            if (!result.Properties.TryGetValue("code", out var code))
                return false;

            var response = await httpClient.PostAsJsonAsync("api/auth/oauth/google", new
            {
                Code = code,
                ClientId = google.ClientId,
                RedirectUri = google.RedirectUri,
                CodeVerifier = codeVerifier
            });

            if (!response.IsSuccessStatusCode)
                return false;

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json, JsonSerializerOptions.Web);
            if (session == null)
            {
                return false;
            }

            session.CurrentLoginMode = LoginMode.Registered;
            await userService.SaveUserSessionAsync(session);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginWithAppleAsync()
    {
        try
        {
            if (DeviceInfo.Platform != DevicePlatform.iOS || DeviceInfo.Version.Major < 13)
            {
                return false;
            }

            var result = await AppleSignInAuthenticator.AuthenticateAsync(new AppleSignInAuthenticator.Options
            {
                IncludeEmailScope = true,
                IncludeFullNameScope = true
            });

            var fullname = result.Properties["name"] ?? string.Empty;
            var email = result.Properties["email"] ?? string.Empty;
            var idToken = result.IdToken;

            var response = await httpClient.PostAsJsonAsync("api/auth/oauth/apple", new
            {
                FullName = fullname,
                Email = email,
                Token = idToken
            });

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json, JsonSerializerOptions.Web);
            if (session == null)
            {
                return false;
            }

            await userService.SaveUserSessionAsync(session);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginWithMicrosoftAsync()
    {
        try
        {
            var pca = PublicClientApplicationBuilder
                .Create(settings.Microsoft.ClientId)
                .WithRedirectUri(settings.Microsoft.RedirectUri)
                .Build();

            AuthenticationResult result;
            try
            {
                var accounts = await pca.GetAccountsAsync();
                result = await pca.AcquireTokenSilent(settings.Microsoft.Scopes, accounts.FirstOrDefault()).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = await pca.AcquireTokenInteractive(settings.Microsoft.Scopes).ExecuteAsync();
            }

            var token = result.AccessToken;
            var response = await httpClient.PostAsJsonAsync("api/auth/oauth/microsoft", new
            {
                Token = token
            });

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<UserSession>(json, JsonSerializerOptions.Web);
            if (session == null)
            {
                return false;
            }

            session.CurrentLoginMode = LoginMode.Registered;
            await userService.SaveUserSessionAsync(session);
            return true;
        }
        catch
        {
            return false;
        }
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

        await userService.SaveUserSessionAsync(session);
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