using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;

namespace ChillNest.MobileApp.ViewModels;

[QueryProperty(nameof(RoomId), "RoomId")]
[QueryProperty(nameof(HotelId), "HotelId")]
public partial class RoomDetailsViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private int roomId;

    [ObservableProperty]
    private int hotelId;

    [ObservableProperty]
    private Room? room;

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

    [ObservableProperty]
    private int nightsCount = 1;

    // Gallery properties
    public string PhotoUrl1 => GetPhotoUrl(0);
    public string PhotoUrl2 => GetPhotoUrl(1);
    public string PhotoUrl3 => GetPhotoUrl(2);
    public string PhotoUrl4 => GetPhotoUrl(3);
    public string RemainingPhotosCount => Room?.Photos != null && Room.Photos.Count > 4
        ? (Room.Photos.Count - 4).ToString()
        : "0";

    public decimal TotalPrice => Room?.Price * NightsCount ?? 0;
    public string TotalPriceDisplay => $"${TotalPrice:F0}";

    private string GetPhotoUrl(int index)
    {
        if (Room?.Photos == null || Room.Photos.Count <= index)
            return "https://via.placeholder.com/300x200";

        var url = Room.Photos[index];
        return !string.IsNullOrEmpty(url) && !url.StartsWith("http")
            ? $"http://10.0.2.2:5182{url}"
            : url ?? "https://via.placeholder.com/300x200";
    }

    public RoomDetailsViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    partial void OnRoomIdChanged(int value)
    {
        Android.Util.Log.Info("RoomDetailsVM", $"🔵 RoomId changed to: {value}");
        if (value > 0 && HotelId > 0)
        {
            _ = LoadRoomDetailsAsync();
        }
    }

    partial void OnHotelIdChanged(int value)
    {
        Android.Util.Log.Info("RoomDetailsVM", $"🔵 HotelId changed to: {value}");
        if (value > 0 && RoomId > 0)
        {
            _ = LoadRoomDetailsAsync();
        }
    }

    partial void OnCheckInDateChanged(DateTime value)
    {
        UpdateNightsCount();
    }

    partial void OnCheckOutDateChanged(DateTime value)
    {
        UpdateNightsCount();
    }

    private void UpdateNightsCount()
    {
        if (CheckOutDate > CheckInDate)
        {
            NightsCount = (CheckOutDate - CheckInDate).Days;
            OnPropertyChanged(nameof(TotalPrice));
            OnPropertyChanged(nameof(TotalPriceDisplay));
        }
    }

    [RelayCommand]
    private async Task LoadRoomDetailsAsync()
    {
        if (IsLoading || RoomId <= 0 || HotelId <= 0) return;

        try
        {
            Android.Util.Log.Info("RoomDetailsVM", $"🔵 Loading room {RoomId} from hotel {HotelId}");
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Load hotel and rooms in parallel
            var hotelTask = _apiService.GetHotelDetailsAsync(HotelId);
            var roomsTask = _apiService.GetHotelRoomsAsync(HotelId);

            await Task.WhenAll(hotelTask, roomsTask);

            Hotel = await hotelTask;
            var rooms = await roomsTask;
            Room = rooms?.FirstOrDefault(r => r.RoomId == RoomId);

            if (Room == null)
            {
                Android.Util.Log.Warn("RoomDetailsVM", "⚠️ Room is NULL");
                ErrorMessage = "Room not found";
            }
            else
            {
                Android.Util.Log.Info("RoomDetailsVM", $"✅ Room loaded: {Room.Name}");

                // Notify property changes for gallery
                OnPropertyChanged(nameof(PhotoUrl1));
                OnPropertyChanged(nameof(PhotoUrl2));
                OnPropertyChanged(nameof(PhotoUrl3));
                OnPropertyChanged(nameof(PhotoUrl4));
                OnPropertyChanged(nameof(RemainingPhotosCount));
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("RoomDetailsVM", $"❌ Exception: {ex.Message}");
            ErrorMessage = $"Failed to load room: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task BookNowAsync()
    {
        if (Room == null || Hotel == null) return;

        try
        {
            // Check if user is logged in
            var isLoggedIn = await _authService.IsLoggedInAsync();
            if (!isLoggedIn)
            {
                Android.Util.Log.Warn("RoomDetailsVM", "⚠️ User not logged in");

                var shouldLogin = await Shell.Current.DisplayAlert(
                    "Login Required",
                    "You need to login to make a booking. Would you like to login now?",
                    "Login",
                    "Cancel"
                );

                if (shouldLogin)
                {
                    await Shell.Current.GoToAsync(nameof(Views.LoginPage));
                }
                return;
            }

            Android.Util.Log.Info("RoomDetailsVM", $"🔵 Navigate to BookingPage - Room: {RoomId}, Total: {TotalPrice}");

            // Navigate to booking page directly
            var parameters = new Dictionary<string, object>
            {
                { "RoomId", RoomId },
                { "HotelId", HotelId },
                { "HotelName", Hotel.Name },
                { "RoomName", Room.Name },
                { "CheckInDate", CheckInDate },
                { "CheckOutDate", CheckOutDate },
                { "PricePerNight", Room.Price },
                { "TotalPrice", TotalPrice }
            };

            await Shell.Current.GoToAsync("BookingPage", parameters);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("RoomDetailsVM", $"❌ Navigation error: {ex.Message}");
            ErrorMessage = $"Navigation failed: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
