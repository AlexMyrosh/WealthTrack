using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Content;

namespace WealthTrack.Client;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
// MSAL redirect handling for Android: msal{bundleId}://auth
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault },
    DataScheme = "msalcom.somemaker.wealthtrack",
    DataHost = "auth")]
public class MainActivity : MauiAppCompatActivity
{
}