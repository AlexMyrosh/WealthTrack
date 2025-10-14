using System.Text.Json.Serialization;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.Models;

public class UserSession
{
    public string Fullname { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public Guid? AccountCurrencyId { get; set; }
    
    public CustomAppTheme? SelectedAppTheme { get; set; }
    
    public bool IsSyncEnabled { get; set; }
    
    public string Token { get; set; } = string.Empty;
    
    public LoginMode CurrentLoginMode { get; set; }
    
    public bool IsIntroductionCompleted { get; set; }
}