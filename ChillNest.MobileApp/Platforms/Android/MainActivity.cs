using Android.App;
using Android.Content.PM;
using Android.OS;

namespace ChillNest.MobileApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        Android.Util.Log.Info("ChillNest", "========== MainActivity OnCreate ==========");
        base.OnCreate(savedInstanceState);
        Android.Util.Log.Info("ChillNest", "========== MainActivity OnCreate DONE ==========");
    }
}
