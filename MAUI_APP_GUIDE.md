# Hướng dẫn tạo .NET MAUI Mobile App cho ChillNest

## ✅ Đã hoàn thành:
1. ✅ Tạo REST API cho mobile app
2. ✅ Thêm CORS configuration
3. ✅ API Documentation

## 📱 Các bước tiếp theo để tạo Mobile App:

### Bước 1: Tạo .NET MAUI Project

```powershell
# Di chuyển đến thư mục solution
cd d:\ChillNestV2\Booking-HomeStay

# Tạo .NET MAUI project mới
dotnet new maui -n ChillNest.MobileApp

# Thêm project vào solution
dotnet sln add ChillNest.MobileApp/ChillNest.MobileApp.csproj
```

### Bước 2: Cài đặt NuGet Packages cần thiết

```powershell
cd ChillNest.MobileApp

# Install packages
dotnet add package CommunityToolkit.Mvvm
dotnet add package CommunityToolkit.Maui
dotnet add package Newtonsoft.Json
```

### Bước 3: Cấu trúc thư mục đề xuất

```
ChillNest.MobileApp/
├── Models/              # Data models
│   ├── Hotel.cs
│   ├── Room.cs
│   ├── Booking.cs
│   └── User.cs
├── Services/            # API services
│   ├── IApiService.cs
│   ├── ApiService.cs
│   └── AuthService.cs
├── ViewModels/          # MVVM ViewModels
│   ├── LoginViewModel.cs
│   ├── HotelsViewModel.cs
│   ├── HotelDetailsViewModel.cs
│   └── BookingsViewModel.cs
├── Views/               # XAML Pages
│   ├── LoginPage.xaml
│   ├── HotelsPage.xaml
│   ├── HotelDetailsPage.xaml
│   └── BookingsPage.xaml
├── Helpers/             # Helper classes
│   └── Constants.cs
└── App.xaml
```

### Bước 4: Tạo Models

**Models/Hotel.cs:**
```csharp
namespace ChillNest.MobileApp.Models
{
    public class Hotel
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public decimal MinPrice { get; set; }
        public string MainPhoto { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();
        public int RoomCount { get; set; }
        public double AvgRating { get; set; }
        public List<Room> Rooms { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }

    public class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string BedType { get; set; } = string.Empty;
        public int Size { get; set; }
        public decimal Price { get; set; }
        public string MainPhoto { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();
        public List<Amenity> Amenities { get; set; } = new();
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class Amenity
    {
        public int AmenityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class Review
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public UserInfo User { get; set; } = new();
        public string RoomName { get; set; } = string.Empty;
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
```

**Models/Booking.cs:**
```csharp
namespace ChillNest.MobileApp.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public RoomInfo Room { get; set; } = new();
        public HotelInfo Hotel { get; set; } = new();
        public int Nights { get; set; }
    }

    public class RoomInfo
    {
        public int RoomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string MainPhoto { get; set; } = string.Empty;
    }

    public class HotelInfo
    {
        public int HotelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}
```

### Bước 5: Tạo ApiService

**Services/IApiService.cs:**
```csharp
using ChillNest.MobileApp.Models;

namespace ChillNest.MobileApp.Services
{
    public interface IApiService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<HotelsResponse> GetHotelsAsync(int page = 1, int pageSize = 10, string? search = null, string? city = null);
        Task<Hotel> GetHotelDetailsAsync(int hotelId);
        Task<List<Booking>> GetUserBookingsAsync();
        Task<Booking> CreateBookingAsync(int roomId, DateTime checkIn, DateTime checkOut, string guestName, string guestPhone);
        Task<bool> CancelBookingAsync(int bookingId);
    }

    public class LoginResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class HotelsResponse
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<Hotel> Data { get; set; } = new();
    }
}
```

**Services/ApiService.cs:**
```csharp
using System.Net.Http.Json;
using Newtonsoft.Json;
using ChillNest.MobileApp.Models;

namespace ChillNest.MobileApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://10.0.2.2:5182/api"; // Android emulator localhost

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var request = new { email, password };
            var response = await _httpClient.PostAsJsonAsync("/AuthApi/login", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<LoginResponse>(content)!;
        }

        public async Task<HotelsResponse> GetHotelsAsync(int page = 1, int pageSize = 10, string? search = null, string? city = null)
        {
            var query = $"?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search)) query += $"&search={search}";
            if (!string.IsNullOrEmpty(city)) query += $"&city={city}";

            var response = await _httpClient.GetAsync($"/HotelsApi{query}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HotelsResponse>(content)!;
        }

        public async Task<Hotel> GetHotelDetailsAsync(int hotelId)
        {
            var response = await _httpClient.GetAsync($"/HotelsApi/{hotelId}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Hotel>(content)!;
        }

        public async Task<List<Booking>> GetUserBookingsAsync()
        {
            var response = await _httpClient.GetAsync("/BookingsApi");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Booking>>(content)!;
        }

        public async Task<Booking> CreateBookingAsync(int roomId, DateTime checkIn, DateTime checkOut, string guestName, string guestPhone)
        {
            var request = new { roomId, checkIn, checkOut, guestName, guestPhone };
            var response = await _httpClient.PostAsJsonAsync("/BookingsApi", request);
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Booking>(content)!;
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var response = await _httpClient.PutAsync($"/BookingsApi/{bookingId}/cancel", null);
            return response.IsSuccessStatusCode;
        }
    }
}
```

### Bước 6: Tạo ViewModel với MVVM

**ViewModels/HotelsViewModel.cs:**
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;
using System.Collections.ObjectModel;

namespace ChillNest.MobileApp.ViewModels
{
    public partial class HotelsViewModel : ObservableObject
    {
        private readonly IApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<Hotel> hotels = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string searchText = string.Empty;

        public HotelsViewModel(IApiService apiService)
        {
            _apiService = apiService;
        }

        [RelayCommand]
        private async Task LoadHotelsAsync()
        {
            try
            {
                IsLoading = true;
                var response = await _apiService.GetHotelsAsync(search: SearchText);
                Hotels = new ObservableCollection<Hotel>(response.Data);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchHotelsAsync()
        {
            await LoadHotelsAsync();
        }

        [RelayCommand]
        private async Task ViewHotelDetailsAsync(Hotel hotel)
        {
            await Shell.Current.GoToAsync($"hoteldetails?id={hotel.HotelId}");
        }
    }
}
```

### Bước 7: Tạo XAML UI

**Views/HotelsPage.xaml:**
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:ChillNest.MobileApp.ViewModels"
             x:Class="ChillNest.MobileApp.Views.HotelsPage"
             Title="Hotels">
    
    <ContentPage.BindingContext>
        <vm:HotelsViewModel />
    </ContentPage.BindingContext>

    <Grid RowDefinitions="Auto,*">
        <!-- Search Bar -->
        <SearchBar Grid.Row="0"
                   Placeholder="Search hotels..."
                   Text="{Binding SearchText}"
                   SearchCommand="{Binding SearchHotelsCommand}" />

        <!-- Hotels List -->
        <CollectionView Grid.Row="1"
                        ItemsSource="{Binding Hotels}"
                        SelectionMode="Single">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Padding="10" Margin="10,5">
                        <Grid ColumnDefinitions="100,*" RowDefinitions="Auto,Auto,Auto">
                            <Image Grid.Column="0" Grid.RowSpan="3"
                                   Source="{Binding MainPhoto}"
                                   Aspect="AspectFill"
                                   HeightRequest="100"
                                   WidthRequest="100" />
                            
                            <Label Grid.Column="1" Grid.Row="0"
                                   Text="{Binding Name}"
                                   FontAttributes="Bold"
                                   FontSize="16" />
                            
                            <Label Grid.Column="1" Grid.Row="1"
                                   Text="{Binding City}"
                                   TextColor="Gray" />
                            
                            <Label Grid.Column="1" Grid.Row="2"
                                   Text="{Binding MinPrice, StringFormat='From {0:N0} ₫'}"
                                   TextColor="Orange"
                                   FontAttributes="Bold" />
                        </Grid>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Loading Indicator -->
        <ActivityIndicator Grid.Row="1"
                          IsRunning="{Binding IsLoading}"
                          IsVisible="{Binding IsLoading}"
                          VerticalOptions="Center"
                          HorizontalOptions="Center" />
    </Grid>
</ContentPage>
```

### Bước 8: Đăng ký Services trong MauiProgram.cs

**MauiProgram.cs:**
```csharp
using ChillNest.MobileApp.Services;
using ChillNest.MobileApp.ViewModels;
using ChillNest.MobileApp.Views;
using CommunityToolkit.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register Services
        builder.Services.AddSingleton<IApiService, ApiService>();

        // Register ViewModels
        builder.Services.AddTransient<HotelsViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<BookingsViewModel>();

        // Register Pages
        builder.Services.AddTransient<HotelsPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<BookingsPage>();

        return builder.Build();
    }
}
```

### Bước 9: Test trên Android

```powershell
# Build và chạy trên Android emulator
dotnet build -t:Run -f net8.0-android
```

---

## 🎯 Features cần implement:

1. ✅ **Authentication**
   - Login
   - Register với OTP
   - Profile management

2. ✅ **Hotels**
   - Danh sách hotels với search & filter
   - Chi tiết hotel
   - Xem rooms của hotel
   - Reviews & ratings

3. ✅ **Bookings**
   - Đặt phòng
   - Xem lịch sử đặt phòng
   - Hủy đặt phòng
   - Check availability

4. **Additional Features** (Optional):
   - Map view với nearby hotels
   - Favorites/Wishlist
   - Push notifications
   - Payment integration
   - Dark mode

---

## 📚 Resources:

- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [API Documentation](./API_DOCUMENTATION.md)

---

## 🚀 Ready to go!

Backend API đã sẵn sàng và đang chạy tại `http://localhost:5182`

Bây giờ bạn có thể:
1. Follow các bước trên để tạo mobile app
2. Test API bằng Postman trước
3. Implement từng feature một
4. Deploy lên Google Play Store khi hoàn thành

Good luck! 🎉
