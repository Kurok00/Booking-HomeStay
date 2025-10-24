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

#if ANDROID
        Android.Util.Log.Info("ChillNest", "[HotelsPage] OnAppearing called!");
#endif

        // Refresh user info every time page appears
        await _viewModel.RefreshUserInfoAsync();

#if ANDROID
        Android.Util.Log.Info("ChillNest", $"[HotelsPage] Current Hotels count: {_viewModel.Hotels.Count}");
#endif

        // Load hotels when page appears
        if (_viewModel.Hotels.Count == 0)
        {
            try
            {
#if ANDROID
                Android.Util.Log.Info("ChillNest", "[HotelsPage] About to load hotels...");
#endif
                await _viewModel.LoadHotelsCommand.ExecuteAsync(null);
#if ANDROID
                Android.Util.Log.Info("ChillNest", $"[HotelsPage] Load completed. Hotels count: {_viewModel.Hotels.Count}");
#endif

                // Show alert if there's an error
                if (!string.IsNullOrEmpty(_viewModel.ErrorMessage))
                {
#if ANDROID
                    Android.Util.Log.Warn("ChillNest", $"[HotelsPage] Error message: {_viewModel.ErrorMessage}");
#endif
                    await DisplayAlert("Error", _viewModel.ErrorMessage, "OK");
                }
            }
            catch (Exception ex)
            {
#if ANDROID
                Android.Util.Log.Error("ChillNest", $"[HotelsPage] Exception: {ex.GetType().Name} - {ex.Message}");
                Android.Util.Log.Error("ChillNest", $"[HotelsPage] Stack trace: {ex.StackTrace}");
#endif
                await DisplayAlert("Error", $"Failed to load hotels:\n{ex.Message}", "OK");
            }
        }
        else
        {
#if ANDROID
            Android.Util.Log.Info("ChillNest", "[HotelsPage] Hotels already loaded, skipping...");
#endif
        }
    }
}