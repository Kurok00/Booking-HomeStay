using ChillNest.MobileApp.ViewModels;

namespace ChillNest.MobileApp.Views;

public partial class HotelsPage : ContentPage
{
    private readonly HotelsViewModel _viewModel;

    public HotelsPage(HotelsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Android.Util.Log.Info("ChillNest", "[HotelsPage] OnAppearing called!");

        // Refresh user info every time page appears
        await _viewModel.RefreshUserInfoAsync();

        Android.Util.Log.Info("ChillNest", $"[HotelsPage] Current Hotels count: {_viewModel.Hotels.Count}");

        // Load hotels when page appears
        if (_viewModel.Hotels.Count == 0)
        {
            try
            {
                Android.Util.Log.Info("ChillNest", "[HotelsPage] About to load hotels...");
                await _viewModel.LoadHotelsCommand.ExecuteAsync(null);
                Android.Util.Log.Info("ChillNest", $"[HotelsPage] Load completed. Hotels count: {_viewModel.Hotels.Count}");

                // Show alert if there's an error
                if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                {
                    Android.Util.Log.Warn("ChillNest", $"[HotelsPage] Error message: {_viewModel.ErrorMessage}");
                    await DisplayAlert("Error", _viewModel.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("ChillNest", $"[HotelsPage] Exception: {ex.GetType().Name} - {ex.Message}");
                Android.Util.Log.Error("ChillNest", $"[HotelsPage] Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Failed to load hotels:\n{ex.Message}", "OK");
            }
        }
        else
        {
            Android.Util.Log.Info("ChillNest", "[HotelsPage] Hotels already loaded, skipping...");
        }
    }
}