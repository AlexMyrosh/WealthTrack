using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WealthTrack.Client.Services;

public class ApiClient
{
    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://localhost:7071") // WealthTrack.API base url
    };

    private async Task AddAuthHeaderAsync()
    {
        var token = await SecureStorage.GetAsync("auth_token");
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        await AddAuthHeaderAsync();

        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return default;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
    {
        await AddAuthHeaderAsync();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
            return default;

        var resultJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(resultJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<bool> PutAsync<T>(string url, T data)
    {
        await AddAuthHeaderAsync();

        var json = JsonSerializer.Serialize(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PutAsync(url, content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(string url)
    {
        await AddAuthHeaderAsync();

        var response = await _http.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }
}