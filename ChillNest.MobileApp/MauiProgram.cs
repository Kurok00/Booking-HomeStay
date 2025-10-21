using Microsoft.Extensions.Logging;
using ChillNest.MobileApp.Services;
using ChillNest.MobileApp.ViewModels;
using ChillNest.MobileApp.Views;
using CommunityToolkit.Maui;

namespace ChillNest.MobileApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register Services
		builder.Services.AddSingleton<IApiService, ApiService>();

		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<RegisterViewModel>();
		builder.Services.AddTransient<HotelsViewModel>();
		builder.Services.AddTransient<HotelDetailsViewModel>();

		// Register Pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<RegisterPage>();
		builder.Services.AddTransient<HotelsPage>();
		builder.Services.AddTransient<HotelDetailsPage>();

		return builder.Build();
	}
}
