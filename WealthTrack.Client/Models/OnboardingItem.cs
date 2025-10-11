namespace WealthTrack.Client.Models;

public class OnboardingItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Features { get; set; } = new();
    public bool ShowFeatures { get; set; }
    public bool ShowSkipButton { get; set; } = true;
    public bool IsCurrentPage { get; set; }
    public bool IsLoginPage { get; set; }
}
