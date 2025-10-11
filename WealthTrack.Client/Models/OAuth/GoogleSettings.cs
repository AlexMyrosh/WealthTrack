namespace WealthTrack.Client.Models;

public class GoogleSettings
{
    public GooglePlatformSettings iOS { get; set; } = new();
    public GooglePlatformSettings Android { get; set; } = new();
    public GooglePlatformSettings Desktop { get; set; } = new();
}