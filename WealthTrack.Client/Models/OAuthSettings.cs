namespace WealthTrack.Client.Models;

public class OAuthSettings
{
    public GoogleSettings Google { get; set; } = new();
    public string BackendBaseUrl { get; set; } = string.Empty;
}