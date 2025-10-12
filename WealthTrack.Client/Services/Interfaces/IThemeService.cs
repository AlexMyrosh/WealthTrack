using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.Services.Interfaces;

public interface IThemeService
{
    public void SetTheme(CustomAppTheme themeName);
}