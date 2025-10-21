namespace ChillNest.MobileApp;

public partial class App : Application
{
	public App()
	{
		Android.Util.Log.Info("ChillNest", "========== App Constructor ==========");
		InitializeComponent();
		Android.Util.Log.Info("ChillNest", "========== App InitializeComponent DONE ==========");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Android.Util.Log.Info("ChillNest", "========== CreateWindow called ==========");
		var appShell = new AppShell();
		Android.Util.Log.Info("ChillNest", "========== AppShell created ==========");
		return new Window(appShell);
	}
}