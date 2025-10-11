using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.Models;

public class UserSession
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Token { get; set; } = string.Empty;
    
    public LoginMode CurrentLoginMode { get; set; }
    
    public bool IsIntroductionCompleted { get; set; }
}