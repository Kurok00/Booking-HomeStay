namespace ChillNest.MobileApp;

public partial class App : Application
{
	public App()
	{
#if ANDROID
		Android.Util.Log.Info("ChillNest", "========== App Constructor ==========");
#endif
		InitializeComponent();
#if ANDROID
		Android.Util.Log.Info("ChillNest", "========== App InitializeComponent DONE ==========");
#endif
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
#if ANDROID
		Android.Util.Log.Info("ChillNest", "========== CreateWindow called ==========");
#endif
		var appShell = new AppShell();
#if ANDROID
		Android.Util.Log.Info("ChillNest", "========== AppShell created ==========");
#endif
		return new Window(appShell);
	}
}