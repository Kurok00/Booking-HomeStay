using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;

namespace ChillNest.MobileApp.ViewModels;

[QueryProperty(nameof(HotelId), "HotelId")]
public partial class HotelDetailsViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private int hotelId;

    [ObservableProperty]
    private Hotel? hotel;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private DateTime checkInDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime checkOutDate = DateTime.Today.AddDays(2);

    public HotelDetailsViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnHotelIdChanged(int value)
    {
        if (value > 0)
        {
            _ = LoadHotelDetailsAsync();
        }
    }

    [RelayCommand]
    private async Task LoadHotelDetailsAsync()
    {
        if (IsLoading || HotelId <= 0) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Hotel = await _apiService.GetHotelDetailsAsync(HotelId);

            if (Hotel == null)
            {
                ErrorMessage = "Hotel not found";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load hotel: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BookRoomAsync(Room room)
    {
        if (room == null || Hotel == null) return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Check availability first
            var available = await _apiService.CheckAvailabilityAsync(room.Id, CheckInDate, CheckOutDate);

            if (!available)
            {
                await Shell.Current.DisplayAlert("Not Available", "This room is not available for the selected dates.", "OK");
                return;
            }

            // Navigate to booking page with parameters
            var parameters = new Dictionary<string, object>
            {
                { "RoomId", room.Id },
                { "HotelName", Hotel.Name },
                { "RoomName", room.Name },
                { "CheckInDate", CheckInDate },
                { "CheckOutDate", CheckOutDate },
                { "PricePerNight", room.PricePerNight }
            };

            await Shell.Current.GoToAsync("BookingPage", parameters);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Booking failed: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectRoomsAsync()
    {
        if (Hotel == null) return;

        // TODO: Navigate to room selection page
        await Shell.Current.DisplayAlert(
            "Select Rooms",
            $"Room selection for {Hotel.Name}\nCheck-in: {CheckInDate:MMM dd}\nCheck-out: {CheckOutDate:MMM dd}\n\nThis feature will show available rooms.",
            "OK"
        );
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
