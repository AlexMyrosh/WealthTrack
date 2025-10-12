using WealthTrack.Client.Services.Interfaces;
using WealthTrack.Shared.Enums;

namespace WealthTrack.Client.Services.Implementations;

public class ThemeService : IThemeService
{
    public void SetTheme(CustomAppTheme theme)
    {
        ResourceDictionary newTheme = theme switch
        {
            CustomAppTheme.Light => new Themes.LightTheme(),
            CustomAppTheme.Dark => new Themes.DarkTheme(),
            CustomAppTheme.Pink => new Themes.PinkTheme(),
            _ => new Themes.LightTheme()
        };
        
        Application.Current.Resources.MergedDictionaries.Clear();
        Application.Current.Resources.MergedDictionaries.Add(newTheme);
    }
}