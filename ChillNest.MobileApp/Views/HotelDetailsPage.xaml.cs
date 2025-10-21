using ChillNest.MobileApp.ViewModels;

namespace ChillNest.MobileApp.Views;

public partial class HotelDetailsPage : ContentPage
{
    public HotelDetailsPage(HotelDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}