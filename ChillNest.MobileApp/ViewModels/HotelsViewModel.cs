using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;
using System.Collections.ObjectModel;

namespace ChillNest.MobileApp.ViewModels;

public partial class HotelsViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<Hotel> hotels = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private string userAvatarDisplay = "👤";

    [ObservableProperty]
    private bool isUserLoggedIn;

    [ObservableProperty]
    private bool hasUserAvatar;

    [ObservableProperty]
    private bool hasNoUserAvatar = true;

    private int _currentPage = 1;
    private const int PageSize = 10;

    public HotelsViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;

        // Load user info when ViewModel is created
        _ = LoadUserInfoAsync();
    }

    public async Task RefreshUserInfoAsync()
    {
        await LoadUserInfoAsync();
    }

    private async Task LoadUserInfoAsync()
    {
        try
        {
            Android.Util.Log.Info("HotelsVM", "🔵 LoadUserInfoAsync called");

            CurrentUser = await _authService.GetCurrentUserAsync();
            if (CurrentUser != null)
            {
                Android.Util.Log.Info("HotelsVM", $"✅ User loaded: {CurrentUser.UserName}");

                IsUserLoggedIn = true;

                // Check if user has real avatar URL (from /Image/avatars/ folder, not external services)
                bool hasRealAvatar = !string.IsNullOrEmpty(CurrentUser.AvatarUrl)
                    && CurrentUser.AvatarUrl.StartsWith("/Image/avatars/");

                HasUserAvatar = hasRealAvatar;
                HasNoUserAvatar = !hasRealAvatar;

                Android.Util.Log.Info("HotelsVM", $"Avatar URL: {CurrentUser.AvatarUrl}");
                Android.Util.Log.Info("HotelsVM", $"Avatar Display: {CurrentUser.AvatarDisplay}");
                Android.Util.Log.Info("HotelsVM", $"HasUserAvatar: {HasUserAvatar}, HasNoUserAvatar: {HasNoUserAvatar}");
            }
            else
            {
                Android.Util.Log.Info("HotelsVM", "⚠️ No user logged in");
                IsUserLoggedIn = false;
                HasUserAvatar = false;
                HasNoUserAvatar = true;
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("HotelsVM", $"❌ Error loading user: {ex.Message}");
            IsUserLoggedIn = false;
            HasUserAvatar = false;
            HasNoUserAvatar = true;
        }
    }

    [RelayCommand]
    private async Task NavigateToProfileAsync()
    {
        try
        {
            var isLoggedIn = await _authService.IsLoggedInAsync();
            if (!isLoggedIn)
            {
                // Show login prompt
                var shouldLogin = await Shell.Current.DisplayAlert(
                    "Login Required",
                    "You need to login to view your profile. Would you like to login now?",
                    "Login",
                    "Cancel"
                );

                if (shouldLogin)
                {
                    await Shell.Current.GoToAsync(nameof(Views.LoginPage));
                }
                return;
            }

            // Navigate to profile page
            await Shell.Current.GoToAsync("ProfilePage");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("HotelsVM", $"❌ Profile navigation error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoadHotelsAsync()
    {
        if (IsLoading) return;

        try
        {
            Android.Util.Log.Info("ChillNest", "[HotelsViewModel] Starting to load hotels...");
            IsLoading = true;
            ErrorMessage = string.Empty;
            _currentPage = 1;

            // Refresh user info when loading hotels
            await LoadUserInfoAsync();

            Android.Util.Log.Info("ChillNest", $"[HotelsViewModel] Calling API - Page: {_currentPage}, PageSize: {PageSize}, Search: {SearchText}");
            var hotels = await _apiService.GetHotelsAsync(_currentPage, PageSize, string.IsNullOrEmpty(SearchText) ? null : SearchText);

            Android.Util.Log.Info("ChillNest", $"[HotelsViewModel] Received {hotels.Count} hotels from API");

            Hotels.Clear();
            foreach (var hotel in hotels)
            {
                Hotels.Add(hotel);
                Android.Util.Log.Debug("ChillNest", $"  Hotel: {hotel.Name} - {hotel.City}");
            }

            Android.Util.Log.Info("ChillNest", $"[HotelsViewModel] Hotels collection now has {Hotels.Count} items");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("ChillNest", $"[HotelsViewModel] ERROR: {ex.GetType().Name} - {ex.Message}");
            Android.Util.Log.Error("ChillNest", $"[HotelsViewModel] Stack trace: {ex.StackTrace}");
            ErrorMessage = $"Failed to load hotels: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            Android.Util.Log.Info("ChillNest", $"[HotelsViewModel] LoadHotels completed. IsLoading = {IsLoading}");
        }
    }

    [RelayCommand]
    private async Task RefreshHotelsAsync()
    {
        if (IsRefreshing) return;

        try
        {
            IsRefreshing = true;
            ErrorMessage = string.Empty;
            _currentPage = 1;

            var hotels = await _apiService.GetHotelsAsync(_currentPage, PageSize, string.IsNullOrEmpty(SearchText) ? null : SearchText);

            Hotels.Clear();
            foreach (var hotel in hotels)
            {
                Hotels.Add(hotel);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to refresh: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SearchHotelsAsync()
    {
        await LoadHotelsAsync();
    }

    [RelayCommand]
    private async Task LoadMoreHotelsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            _currentPage++;

            var hotels = await _apiService.GetHotelsAsync(_currentPage, PageSize, string.IsNullOrEmpty(SearchText) ? null : SearchText);

            foreach (var hotel in hotels)
            {
                Hotels.Add(hotel);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load more: {ex.Message}";
            _currentPage--; // Revert page increment on error
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToHotelDetailsAsync(Hotel hotel)
    {
        if (hotel == null) return;

        var parameters = new Dictionary<string, object>
        {
            { "HotelId", hotel.Id }
        };

        await Shell.Current.GoToAsync($"HotelDetailsPage", parameters);
    }
}
