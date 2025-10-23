using ChillNest.MobileApp.ViewModels;

namespace ChillNest.MobileApp.Views;

public partial class RoomDetailsPage : ContentPage
{
    public RoomDetailsPage(RoomDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
