using ChillNest.MobileApp.Views;

namespace ChillNest.MobileApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register routes for navigation
		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
		Routing.RegisterRoute(nameof(HotelDetailsPage), typeof(HotelDetailsPage));
		Routing.RegisterRoute(nameof(RoomDetailsPage), typeof(RoomDetailsPage));
		Routing.RegisterRoute(nameof(BookingPage), typeof(BookingPage));
	}
}
