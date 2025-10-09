namespace WealthTrack.Client.Models;

public class MicrosoftSettings
{
    public string ClientId { get; set; } = string.Empty;
    
    public string RedirectUri { get; set; } = string.Empty;
    
    public string[] Scopes { get; set; } = [];
}