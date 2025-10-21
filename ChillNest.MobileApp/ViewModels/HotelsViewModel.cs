using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;
using System.Collections.ObjectModel;

namespace ChillNest.MobileApp.ViewModels;

public partial class HotelsViewModel : ObservableObject
{
    private readonly IApiService _apiService;

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

    private int _currentPage = 1;
    private const int PageSize = 10;

    public HotelsViewModel(IApiService apiService)
    {
        _apiService = apiService;
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
