using ChillNest.MobileApp.Models;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace ChillNest.MobileApp.Services;

public interface IApiService
{
    // Hotels
    Task<List<Hotel>> GetHotelsAsync(int page = 1, int pageSize = 10, string? searchTerm = null);
    Task<Hotel?> GetHotelDetailsAsync(int hotelId);
    Task<List<Hotel>> GetNearbyHotelsAsync(double latitude, double longitude, double radiusKm = 10);

    // Auth
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<User?> GetProfileAsync();
    Task<bool> LogoutAsync();

    // Bookings
    Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
    Task<List<Booking>> GetMyBookingsAsync();
    Task<bool> CancelBookingAsync(int bookingId);
    Task<bool> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    // IMPORTANT: Use 10.0.2.2 for Android Emulator to access localhost
    // BaseAddress MUST end with / for proper URL combination!
    private const string BaseUrl = "http://10.0.2.2:5182/api/";

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    #region Hotels

    public async Task<List<Hotel>> GetHotelsAsync(int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrEmpty(searchTerm))
                queryParams.Add($"search={Uri.EscapeDataString(searchTerm)}");

            var query = string.Join("&", queryParams);
            var url = $"HotelsApi?{query}"; // NO leading slash when BaseAddress ends with /
            var fullUrl = $"{BaseUrl}{url}";

            Android.Util.Log.Info("ChillNest", $"[ApiService] GET {fullUrl}");
            Android.Util.Log.Info("ChillNest", $"[ApiService] BaseAddress: {_httpClient.BaseAddress}");
            Android.Util.Log.Info("ChillNest", $"[ApiService] Request URL: {url}");

            var response = await _httpClient.GetAsync(url);

            Android.Util.Log.Info("ChillNest", $"[ApiService] Response Status: {response.StatusCode}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Android.Util.Log.Info("ChillNest", $"[ApiService] Response length: {content.Length} chars");
            Android.Util.Log.Info("ChillNest", $"[ApiService] First 200 chars: {content.Substring(0, Math.Min(200, content.Length))}");

            var apiResponse = JsonConvert.DeserializeObject<HotelsApiResponse>(content);
            Android.Util.Log.Info("ChillNest", $"[ApiService] Deserialized {apiResponse?.Data?.Count ?? 0} hotels");

            return apiResponse?.Data ?? new List<Hotel>();
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("ChillNest", $"[ApiService] ERROR: {ex.GetType().Name} - {ex.Message}");
            Android.Util.Log.Error("ChillNest", $"[ApiService] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Android.Util.Log.Error("ChillNest", $"[ApiService] Inner Exception: {ex.InnerException.Message}");
            }
            throw; // Re-throw to show error in UI
        }
    }

    private class HotelsApiResponse
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<Hotel> Data { get; set; } = new();
    }

    public async Task<Hotel?> GetHotelDetailsAsync(int hotelId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"HotelsApi/{hotelId}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Hotel>(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting hotel details: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Hotel>> GetNearbyHotelsAsync(double latitude, double longitude, double radiusKm = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"HotelsApi/nearby?latitude={latitude}&longitude={longitude}&radiusKm={radiusKm}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Hotel>>(content) ?? new List<Hotel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting nearby hotels: {ex.Message}");
            return new List<Hotel>();
        }
    }

    #endregion

    #region Auth

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("AuthApi/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<LoginResponse>(responseContent)
                ?? new LoginResponse { Success = false, Message = "Invalid response from server" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during login: {ex.Message}");
            return new LoginResponse { Success = false, Message = $"Connection error: {ex.Message}" };
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("AuthApi/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<RegisterResponse>(responseContent)
                ?? new RegisterResponse { Success = false, Message = "Invalid response from server" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during registration: {ex.Message}");
            return new RegisterResponse { Success = false, Message = $"Connection error: {ex.Message}" };
        }
    }

    public async Task<User?> GetProfileAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("AuthApi/profile");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<User>(content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting profile: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("AuthApi/logout", null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Bookings

    public async Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request)
    {
        try
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("BookingsApi", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BookingResponse>(responseContent)
                ?? new BookingResponse { Success = false, Message = "Invalid response from server" };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating booking: {ex.Message}");
            return new BookingResponse { Success = false, Message = $"Connection error: {ex.Message}" };
        }
    }

    public async Task<List<Booking>> GetMyBookingsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("BookingsApi/my-bookings");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Booking>>(content) ?? new List<Booking>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting bookings: {ex.Message}");
            return new List<Booking>();
        }
    }

    public async Task<bool> CancelBookingAsync(int bookingId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/BookingsApi/{bookingId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling booking: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        try
        {
            var checkInStr = checkIn.ToString("yyyy-MM-dd");
            var checkOutStr = checkOut.ToString("yyyy-MM-dd");

            var response = await _httpClient.GetAsync($"/BookingsApi/check-availability?roomId={roomId}&checkInDate={checkInStr}&checkOutDate={checkOutStr}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(content);
                return result?.available ?? false;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking availability: {ex.Message}");
            return false;
        }
    }

    #endregion
}
