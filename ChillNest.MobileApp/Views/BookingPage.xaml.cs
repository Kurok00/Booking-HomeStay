using ChillNest.MobileApp.ViewModels;

namespace ChillNest.MobileApp.Views;

public partial class BookingPage : ContentPage
{
    private readonly BookingViewModel _viewModel;

    public BookingPage(BookingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(); // Load vouchers when page appears
    }
}
