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
    Task<List<Room>> GetHotelRoomsAsync(int hotelId);
    Task<List<Review>> GetHotelReviewsAsync(int hotelId);

    // Auth
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<User?> GetProfileAsync();
    Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);
    Task<bool> LogoutAsync();

    // Bookings
    Task<BookingResponse> CreateBookingAsync(CreateBookingRequest request);
    Task<List<Booking>> GetMyBookingsAsync();
    Task<List<BookingItem>> GetUserBookingsAsync(int userId); // ✅ New method
    Task<bool> CancelBookingAsync(int bookingId, int userId); // ✅ Updated with userId
    Task<bool> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut);

    // Vouchers
    Task<List<VoucherModel>> GetAvailableVouchersAsync();
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


#if ANDROID
            Android.Util.Log.Info("ChillNest", $"[ApiService] GET {fullUrl}");
            Android.Util.Log.Info("ChillNest", $"[ApiService] BaseAddress: {_httpClient.BaseAddress}");
            Android.Util.Log.Info("ChillNest", $"[ApiService] Request URL: {url}");
#endif

            var response = await _httpClient.GetAsync(url);


#if ANDROID
            Android.Util.Log.Info("ChillNest", $"[ApiService] Response Status: {response.StatusCode}");
#endif

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

#if ANDROID
            Android.Util.Log.Info("ChillNest", $"[ApiService] Response length: {content.Length} chars");
            Android.Util.Log.Info("ChillNest", $"[ApiService] First 200 chars: {content.Substring(0, Math.Min(200, content.Length))}");
#endif

            var apiResponse = JsonConvert.DeserializeObject<HotelsApiResponse>(content);

#if ANDROID
            Android.Util.Log.Info("ChillNest", $"[ApiService] Deserialized {apiResponse?.Data?.Count ?? 0} hotels");
#endif

            return apiResponse?.Data ?? new List<Hotel>();
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ChillNest", $"[ApiService] ERROR: {ex.GetType().Name} - {ex.Message}");
            Android.Util.Log.Error("ChillNest", $"[ApiService] Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Android.Util.Log.Error("ChillNest", $"[ApiService] Inner Exception: {ex.InnerException.Message}");
            }
#endif
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
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 GetHotelDetailsAsync called with hotelId: {hotelId}");
#endif
            var url = $"HotelsApi/{hotelId}";
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Calling URL: {_httpClient.BaseAddress}{url}");
#endif

            var response = await _httpClient.GetAsync(url);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Response status: {response.StatusCode}");
#endif

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Response content length: {content.Length}");
#endif

            var hotel = JsonConvert.DeserializeObject<Hotel>(content);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Deserialized hotel: {hotel?.Name ?? "NULL"}");
#endif

            return hotel;
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error getting hotel details: {ex.Message}");
            Android.Util.Log.Error("ApiService", $"❌ Stack trace: {ex.StackTrace}");
#endif
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

    public async Task<List<Room>> GetHotelRoomsAsync(int hotelId)
    {
        try
        {
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 GetHotelRoomsAsync called with hotelId: {hotelId}");
#endif
            var url = $"HotelsApi/{hotelId}/rooms";

            var response = await _httpClient.GetAsync(url);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Rooms response status: {response.StatusCode}");
#endif

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Rooms content length: {content.Length}");
#endif

            var rooms = JsonConvert.DeserializeObject<List<Room>>(content);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Deserialized {rooms?.Count ?? 0} rooms");
#endif

            return rooms ?? new List<Room>();
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error getting rooms: {ex.Message}");
#endif
            return new List<Room>();
        }
    }

    public async Task<List<Review>> GetHotelReviewsAsync(int hotelId)
    {
        try
        {
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 GetHotelReviewsAsync called with hotelId: {hotelId}");
#endif
            var url = $"HotelsApi/{hotelId}/reviews";

            var response = await _httpClient.GetAsync(url);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Reviews response status: {response.StatusCode}");
#endif

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Reviews content length: {content.Length}");
#endif


            var reviews = JsonConvert.DeserializeObject<List<Review>>(content);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Deserialized {reviews?.Count ?? 0} reviews");
#endif

            return reviews ?? new List<Review>();
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error getting reviews: {ex.Message}");
#endif
            return new List<Review>();
        }
    }

    #endregion

    #region Auth

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Login - Email: {request.Email}");
#endif

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("AuthApi/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

#if ANDROID
            Android.Util.Log.Info("ApiService", $"Response Status: {response.StatusCode}");
            Android.Util.Log.Info("ApiService", $"Response: {responseContent}");
#endif

            if (!response.IsSuccessStatusCode)
            {
                var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var errorMessage = errorObj?.message?.ToString() ?? "Login failed";
                return new LoginResponse { Success = false, Message = errorMessage };
            }

            // Parse backend response: { userId, userName, email, fullName, phone, avatarUrl, roles, message }
            var backendResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
            var user = new User
            {
                Id = (int)(backendResponse?.userId ?? 0),
                UserName = backendResponse?.userName?.ToString() ?? "",
                Email = backendResponse?.email?.ToString() ?? "",
                Phone = backendResponse?.phone?.ToString(),
                FullName = backendResponse?.fullName?.ToString(),
                AvatarUrl = backendResponse?.avatarUrl?.ToString(),
                Roles = backendResponse?.roles != null
                    ? JsonConvert.DeserializeObject<List<string>>(backendResponse.roles.ToString()) ?? new List<string>()
                    : new List<string>()
            };

            return new LoginResponse
            {
                Success = true,
                Message = backendResponse?.message?.ToString() ?? "Login successful",
                User = user
            };
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Login error: {ex.Message}");
#endif
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
            // Get auth token
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
#if ANDROID
                Android.Util.Log.Warn("ApiService", $"⚠️ No auth token found for GetProfile!");
#endif
                return null;
            }

#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 GetProfile - Token: {token.Substring(0, Math.Min(20, token.Length))}...");
#endif

            // Create request with Authorization header
            var request = new HttpRequestMessage(HttpMethod.Get, "AuthApi/profile");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

#if ANDROID
            Android.Util.Log.Info("ApiService", $"Response Status: {response.StatusCode}");
            Android.Util.Log.Info("ApiService", $"Response Content: {content.Substring(0, Math.Min(200, content.Length))}...");
#endif

            if (!response.IsSuccessStatusCode)
            {
#if ANDROID
                Android.Util.Log.Error("ApiService", $"❌ GetProfile failed: {response.StatusCode}");
#endif
                return null;
            }

            // Parse response - backend returns { userId, userName, email, fullName, phone, avatarUrl, createdAt, roles }
            var profileData = JsonConvert.DeserializeObject<dynamic>(content);

            var user = new User
            {
                Id = (int)(profileData?.userId ?? 0),
                UserName = profileData?.userName?.ToString() ?? "",
                Email = profileData?.email?.ToString() ?? "",
                FullName = profileData?.fullName?.ToString(),
                Phone = profileData?.phone?.ToString(),
                AvatarUrl = profileData?.avatarUrl?.ToString(),
                Roles = profileData?.roles != null
                    ? JsonConvert.DeserializeObject<List<string>>(profileData.roles.ToString()) ?? new List<string>()
                    : new List<string>()
            };

#if ANDROID
            Android.Util.Log.Info("ApiService", $"✅ Profile loaded: {user.UserName}, FullName: {user.FullName}");
#endif

            return user;
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error getting profile: {ex.Message}");
#endif
            return null;
        }
    }

    public async Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            // Get auth token
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = "Please login to update profile"
                };
            }

            var json = JsonConvert.SerializeObject(request);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Updating profile - Username: {request.UserName}, FullName: {request.FullName}");
#endif

            // Create request with Authorization header
            var httpRequest = new HttpRequestMessage(HttpMethod.Put, "AuthApi/profile");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseJson);

#if ANDROID
                Android.Util.Log.Info("ApiService", $"✅ Profile updated successfully");
#endif

                return new UpdateProfileResponse
                {
                    Success = true,
                    Message = result?.message ?? "Profile updated successfully"
                };
            }

#if ANDROID
            Android.Util.Log.Warn("ApiService", $"⚠️ Update profile failed: {response.StatusCode}");
#endif
            return new UpdateProfileResponse
            {
                Success = false,
                Message = $"Failed to update profile: {response.StatusCode}"
            };
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Exception updating profile: {ex.Message}");
#endif
            return new UpdateProfileResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
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
#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Creating booking for Room {request.RoomId}");
#endif

            // Get auth token
            var token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token))
            {
#if ANDROID
                Android.Util.Log.Warn("ApiService", $"⚠️ No auth token found!");
#endif
                return new BookingResponse { Success = false, Message = "Please login to create booking" };
            }

#if ANDROID
            Android.Util.Log.Info("ApiService", $"✅ Token found: {token.Substring(0, Math.Min(20, token.Length))}...");
#endif

            var json = JsonConvert.SerializeObject(request);
#if ANDROID
            Android.Util.Log.Info("ApiService", $"Request JSON: {json}");
#endif

            // Create request with Authorization header
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "BookingsApi");
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

#if ANDROID
            Android.Util.Log.Info("ApiService", $"📤 Sending request to: {_httpClient.BaseAddress}BookingsApi");
            Android.Util.Log.Info("ApiService", $"🔐 Authorization header: Bearer {token.Substring(0, Math.Min(20, token.Length))}...");
#endif

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

#if ANDROID
            Android.Util.Log.Info("ApiService", $"Response Status: {response.StatusCode}");
            Android.Util.Log.Info("ApiService", $"Response (first 500 chars): {responseContent.Substring(0, Math.Min(500, responseContent.Length))}");
#endif

            if (!response.IsSuccessStatusCode)
            {
#if ANDROID
                Android.Util.Log.Error("ApiService", $"❌ Booking failed with status: {response.StatusCode}");
#endif

                // Try to parse error message
                try
                {
                    var errorObj = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    var errorMessage = errorObj?.message?.ToString() ?? $"Server error: {response.StatusCode}";
                    return new BookingResponse { Success = false, Message = errorMessage };
                }
                catch
                {
                    return new BookingResponse { Success = false, Message = $"Server error: {response.StatusCode}" };
                }
            }

            // Parse backend response: { bookingId, checkIn, checkOut, totalPrice, status, message, hotelName, roomName, nights }
            var backendResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);

#if ANDROID
            Android.Util.Log.Info("ApiService", $"✅ Booking created successfully - ID: {backendResponse?.bookingId}");
#endif

            return new BookingResponse
            {
                Success = true,
                Message = backendResponse?.message?.ToString() ?? "Booking created successfully",
                BookingId = (int)(backendResponse?.bookingId ?? 0)
            };
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error creating booking: {ex.Message}");
            Android.Util.Log.Error("ApiService", $"Stack trace: {ex.StackTrace}");
#endif
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

    // ✅ Get user bookings list
    public async Task<List<BookingItem>> GetUserBookingsAsync(int userId)
    {
        try
        {
#if ANDROID
            Android.Util.Log.Info("ApiService", $"📥 Getting bookings for userId: {userId}");
#endif

            var response = await _httpClient.GetAsync($"BookingsApi?userId={userId}");

#if ANDROID
            Android.Util.Log.Info("ApiService", $"Response status: {response.StatusCode}");
#endif

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
#if ANDROID
                Android.Util.Log.Info("ApiService", $"Response JSON (first 500 chars): {json.Substring(0, Math.Min(500, json.Length))}");
#endif

                // Parse the response - backend returns array of objects with nested Room and Hotel
                var apiResponse = JsonConvert.DeserializeObject<List<BookingApiResponse>>(json);

                // Map to BookingItem
                var bookings = apiResponse?.Select(b =>
                {
                    // Convert relative photo path to full URL
                    var photoUrl = b.Room?.MainPhoto ?? "";
                    if (!string.IsNullOrEmpty(photoUrl) && !photoUrl.StartsWith("http"))
                    {
                        // Remove leading slash if present
                        photoUrl = photoUrl.TrimStart('/');
                        photoUrl = $"http://10.0.2.2:5182/{photoUrl}";
                    }
                    if (string.IsNullOrEmpty(photoUrl))
                    {
                        photoUrl = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=400";
                    }

                    return new BookingItem
                    {
                        BookingId = b.BookingId,
                        HotelName = b.Hotel?.Name ?? "Unknown Hotel",
                        RoomName = b.Room?.Name ?? "Unknown Room",
                        RoomPhoto = photoUrl,
                        CheckIn = b.CheckIn,
                        CheckOut = b.CheckOut,
                        TotalPrice = b.TotalPrice,
                        StatusString = b.Status,
                        CreatedAt = b.CreatedAt
                    };
                }).ToList() ?? new List<BookingItem>();

#if ANDROID
                Android.Util.Log.Info("ApiService", $"✅ Parsed {bookings.Count} bookings");
#endif

                // Debug: Log first booking details
#if ANDROID
                if (bookings.Any())
                {
                    var first = bookings.First();
                    Android.Util.Log.Info("ApiService", $"📋 First booking: ID={first.BookingId}, Hotel={first.HotelName}, Room={first.RoomName}, Photo={first.RoomPhoto}");
                    Android.Util.Log.Info("ApiService", $"📋 Dates: {first.CheckIn:yyyy-MM-dd} to {first.CheckOut:yyyy-MM-dd}, Total={first.TotalPrice}, Status={first.StatusString}");
                }
#endif

                return bookings;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
#if ANDROID
                Android.Util.Log.Error("ApiService", $"❌ API error: {response.StatusCode} - {errorContent}");
#endif
            }

            return new List<BookingItem>();
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error getting bookings: {ex.Message}");
            Android.Util.Log.Error("ApiService", $"Stack trace: {ex.StackTrace}");
#endif
            return new List<BookingItem>();
        }
    }

    // ✅ Cancel booking with userId
    public async Task<bool> CancelBookingAsync(int bookingId, int userId)
    {
        try
        {
#if ANDROID
            Android.Util.Log.Info("ApiService", $"Canceling booking {bookingId} for userId: {userId}");
#endif

            // Call cancel endpoint (to be implemented in backend)
            var response = await _httpClient.PostAsync($"BookingsApi/{bookingId}/cancel?userId={userId}", null);

            if (response.IsSuccessStatusCode)
            {
#if ANDROID
                Android.Util.Log.Info("ApiService", "✅ Booking canceled successfully");
#endif
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
#if ANDROID
                Android.Util.Log.Error("ApiService", $"❌ Cancel failed: {response.StatusCode} - {errorContent}");
#endif
                return false;
            }
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error canceling booking: {ex.Message}");
#endif
            return false;
        }
    }

    #endregion

    #region Vouchers

    public async Task<List<VoucherModel>> GetAvailableVouchersAsync()
    {
        try
        {
            // Try to get auth token (optional)
            var token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

#if ANDROID
                Android.Util.Log.Info("ApiService", "🔵 Getting vouchers with auth token");
#endif
            }
            else
            {
#if ANDROID
                Android.Util.Log.Info("ApiService", "🔵 Getting vouchers without auth (all vouchers)");
#endif
            }

            var response = await _httpClient.GetAsync("BookingsApi/available-vouchers");

#if ANDROID
            Android.Util.Log.Info("ApiService", $"🔵 Vouchers response status: {response.StatusCode}");
#endif

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
#if ANDROID
                Android.Util.Log.Info("ApiService", $"🔵 Vouchers JSON: {json.Substring(0, Math.Min(200, json.Length))}...");
#endif

                var vouchers = JsonConvert.DeserializeObject<List<VoucherModel>>(json);
#if ANDROID
                Android.Util.Log.Info("ApiService", $"✅ Loaded {vouchers?.Count ?? 0} vouchers");
#endif
                return vouchers ?? new List<VoucherModel>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
#if ANDROID
                Android.Util.Log.Warn("ApiService", $"⚠️ Vouchers API failed: {response.StatusCode} - {errorContent}");
#endif
            }

            return new List<VoucherModel>();
        }
        catch (Exception ex)
        {
#if ANDROID
            Android.Util.Log.Error("ApiService", $"❌ Error getting vouchers: {ex.Message}");
            Android.Util.Log.Error("ApiService", $"❌ Stack trace: {ex.StackTrace}");
#endif
            return new List<VoucherModel>();
        }
    }

    #endregion
}
