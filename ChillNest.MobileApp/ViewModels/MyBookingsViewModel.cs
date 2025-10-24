using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;
using System.Collections.ObjectModel;

namespace ChillNest.MobileApp.ViewModels;

public partial class MyBookingsViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private ObservableCollection<BookingItem> bookings = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool hasBookings;

    public MyBookingsViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        await LoadBookingsAsync();
    }

    [RelayCommand]
    private async Task LoadBookingsAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Get current user
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
#if ANDROID
                Android.Util.Log.Warn("MyBookingsVM", "User not logged in");
#endif
                Bookings.Clear();
                HasBookings = false;
                return;
            }

#if ANDROID
            Android.Util.Log.Info("MyBookingsVM", $"Loading bookings for userId: {currentUser.Id}");
#endif

            // Get bookings from API
            var bookingsList = await _apiService.GetUserBookingsAsync(currentUser.Id);

            Bookings.Clear();
            foreach (var booking in bookingsList)
            {
                Bookings.Add(booking);
            }

            HasBookings = Bookings.Any();

#if ANDROID
            Android.Util.Log.Info("MyBookingsVM", $"✅ Loaded {Bookings.Count} bookings");
#endif
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("MyBookingsVM", $"❌ Error loading bookings: {ex.Message}");
#endif
            ErrorMessage = "Failed to load bookings. Please try again.";
            HasBookings = false;
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadBookingsAsync();
    }

    [RelayCommand]
    private async Task ViewBookingDetailsAsync(BookingItem booking)
    {
        if (booking == null) return;

#if ANDROID
        Android.Util.Log.Info("MyBookingsVM", $"Viewing booking details: {booking.BookingId}");
#endif

        // Navigate to booking details (to be implemented)
        await Shell.Current.DisplayAlert(
            "Booking Details",
            $"Hotel: {booking.HotelName}\n" +
            $"Room: {booking.RoomName}\n" +
            $"Check-in: {booking.CheckInDisplay}\n" +
            $"Check-out: {booking.CheckOutDisplay}\n" +
            $"Status: {booking.StatusDisplay}\n" +
            $"Total: {booking.TotalDisplay}",
            "OK");
    }

    [RelayCommand]
    private async Task CancelBookingAsync(BookingItem booking)
    {
        if (booking == null) return;

        // Check if booking can be canceled
        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Confirmed)
        {
            await Shell.Current.DisplayAlert("Cannot Cancel",
                "Only Pending or Confirmed bookings can be canceled.", "OK");
            return;
        }

        var confirmed = await Shell.Current.DisplayAlert(
            "Cancel Booking",
            $"Are you sure you want to cancel this booking?\n\nHotel: {booking.HotelName}\nRoom: {booking.RoomName}",
            "Yes, Cancel",
            "No");

        if (!confirmed) return;

        try
        {
            IsLoading = true;

            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                await Shell.Current.DisplayAlert("Error", "Please login again", "OK");
                return;
            }

            // Call cancel API (to be implemented in ApiService)
            var success = await _apiService.CancelBookingAsync(booking.BookingId, currentUser.Id);

            if (success)
            {
                await Shell.Current.DisplayAlert("Success", "Booking canceled successfully", "OK");
                await LoadBookingsAsync(); // Refresh list
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to cancel booking", "OK");
            }
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("MyBookingsVM", $"Error canceling booking: {ex.Message}");
#endif
            await Shell.Current.DisplayAlert("Error", "An error occurred while canceling booking", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
