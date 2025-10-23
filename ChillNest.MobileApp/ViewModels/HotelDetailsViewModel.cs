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
    private List<Room> rooms = new();

    [ObservableProperty]
    private List<Review> reviews = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private DateTime checkInDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime checkOutDate = DateTime.Today.AddDays(2);

    // Gallery properties
    public string PhotoUrl1 => GetPhotoUrl(0);
    public string PhotoUrl2 => GetPhotoUrl(1);
    public string PhotoUrl3 => GetPhotoUrl(2);
    public string PhotoUrl4 => GetPhotoUrl(3);
    public string RemainingPhotosCount => Hotel?.PhotoUrls != null && Hotel.PhotoUrls.Count > 4
        ? (Hotel.PhotoUrls.Count - 4).ToString()
        : "0";

    private string GetPhotoUrl(int index)
    {
        if (Hotel?.PhotoUrls == null || Hotel.PhotoUrls.Count <= index)
            return Hotel?.MainPhotoUrl ?? "https://via.placeholder.com/300x200";

        var url = Hotel.PhotoUrls[index];
        return !string.IsNullOrEmpty(url) && !url.StartsWith("http")
            ? $"http://10.0.2.2:5182{url}"
            : url ?? "https://via.placeholder.com/300x200";
    }

    public HotelDetailsViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    partial void OnHotelIdChanged(int value)
    {
        Android.Util.Log.Info("HotelDetailsVM", $"🔵 HotelId changed to: {value}");
        if (value > 0)
        {
            _ = LoadHotelDetailsAsync();
        }
        else
        {
            Android.Util.Log.Warn("HotelDetailsVM", $"⚠️ Invalid HotelId: {value}");
        }
    }

    [RelayCommand]
    private async Task LoadHotelDetailsAsync()
    {
        if (IsLoading || HotelId <= 0) return;

        try
        {
            Android.Util.Log.Info("HotelDetailsVM", $"🔵 LoadHotelDetailsAsync started for HotelId: {HotelId}");
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Load hotel details, rooms, and reviews in parallel
            var hotelTask = _apiService.GetHotelDetailsAsync(HotelId);
            var roomsTask = _apiService.GetHotelRoomsAsync(HotelId);
            var reviewsTask = _apiService.GetHotelReviewsAsync(HotelId);

            await Task.WhenAll(hotelTask, roomsTask, reviewsTask);

            Hotel = await hotelTask;
            Rooms = await roomsTask ?? new List<Room>();
            Reviews = await reviewsTask ?? new List<Review>();

            if (Hotel == null)
            {
                Android.Util.Log.Warn("HotelDetailsVM", "⚠️ Hotel is NULL after API call");
                ErrorMessage = "Hotel not found";
            }
            else
            {
                Android.Util.Log.Info("HotelDetailsVM", $"✅ Hotel loaded: {Hotel.Name} (ID: {Hotel.Id})");
                Android.Util.Log.Info("HotelDetailsVM", $"✅ Rooms loaded: {Rooms.Count}");
                Android.Util.Log.Info("HotelDetailsVM", $"✅ Reviews loaded: {Reviews.Count}");

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
            Android.Util.Log.Error("HotelDetailsVM", $"❌ Exception: {ex.Message}");
            Android.Util.Log.Error("HotelDetailsVM", $"❌ Stack: {ex.StackTrace}");
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
            Android.Util.Log.Info("HotelDetailsVM", $"🔵 Navigate to RoomDetailsPage - Room: {room.RoomId}, Hotel: {Hotel.Id}");

            // Navigate to room details page
            var parameters = new Dictionary<string, object>
            {
                { "RoomId", room.RoomId },
                { "HotelId", Hotel.Id }
            };

            await Shell.Current.GoToAsync(nameof(Views.RoomDetailsPage), parameters);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("HotelDetailsVM", $"❌ Navigate error: {ex.Message}");
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
