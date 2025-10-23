using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;
using System.Collections.ObjectModel;

namespace ChillNest.MobileApp.ViewModels;

[QueryProperty(nameof(RoomId), "RoomId")]
[QueryProperty(nameof(HotelId), "HotelId")]
[QueryProperty(nameof(HotelName), "HotelName")]
[QueryProperty(nameof(RoomName), "RoomName")]
[QueryProperty(nameof(CheckInDate), "CheckInDate")]
[QueryProperty(nameof(CheckOutDate), "CheckOutDate")]
[QueryProperty(nameof(PricePerNight), "PricePerNight")]
[QueryProperty(nameof(TotalPrice), "TotalPrice")]
public partial class BookingViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private int roomId;

    [ObservableProperty]
    private int hotelId;

    [ObservableProperty]
    private string hotelName = string.Empty;

    [ObservableProperty]
    private string roomName = string.Empty;

    [ObservableProperty]
    private DateTime checkInDate = DateTime.Today.AddDays(1);

    [ObservableProperty]
    private DateTime checkOutDate = DateTime.Today.AddDays(2);

    [ObservableProperty]
    private decimal pricePerNight;

    [ObservableProperty]
    private decimal totalPrice;

    [ObservableProperty]
    private string guestName = string.Empty;

    [ObservableProperty]
    private string guestPhone = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<VoucherModel> availableVouchers = new();

    [ObservableProperty]
    private VoucherModel? selectedVoucher;

    [ObservableProperty]
    private decimal discount = 0;

    [ObservableProperty]
    private ObservableCollection<PaymentMethod> paymentMethods = new();

    [ObservableProperty]
    private PaymentMethod? selectedPaymentMethod;

    public decimal SubTotal => PricePerNight * NightsCount;
    public decimal FinalTotal => SubTotal - Discount;
    public int NightsCount => (CheckOutDate - CheckInDate).Days;
    public string NightsDisplay => $"{NightsCount} {(NightsCount > 1 ? "nights" : "night")}";
    public string CheckInDisplay => CheckInDate.ToString("MMM dd, yyyy");
    public string CheckOutDisplay => CheckOutDate.ToString("MMM dd, yyyy");
    public string PricePerNightDisplay => $"${PricePerNight:F0}";
    public string SubTotalDisplay => $"${SubTotal:F0}";
    public string DiscountDisplay => Discount > 0 ? $"-${Discount:F0}" : "$0";
    public string FinalTotalDisplay => $"${FinalTotal:F0}";

    public BookingViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        // Load payment methods
        LoadPaymentMethods();

        // Auto-fill guest info from logged-in user
        await LoadUserInfoAsync();

        // Load available vouchers
        await LoadAvailableVouchersAsync();
    }

    private void LoadPaymentMethods()
    {
        PaymentMethods = new ObservableCollection<PaymentMethod>
        {
            new PaymentMethod
            {
                Code = "COD",
                Name = "Cash on Delivery",
                Description = "Pay when you check-in",
                Icon = "💵"
            },
            new PaymentMethod
            {
                Code = "ONLINE",
                Name = "Online Payment",
                Description = "Pay online now",
                Icon = "💳"
            }
        };

        // Set default to COD
        SelectedPaymentMethod = PaymentMethods.FirstOrDefault();
    }

    private async Task LoadUserInfoAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user != null)
            {
                // Auto-fill name and phone from user profile
                GuestName = user.FullName ?? user.UserName;
                GuestPhone = user.Phone ?? "";

                Android.Util.Log.Info("BookingVM", $"✅ Auto-filled guest info - Name: {GuestName}, Phone: {GuestPhone}");
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("BookingVM", $"⚠️ Failed to load user info: {ex.Message}");
        }
    }

    private async Task LoadAvailableVouchersAsync()
    {
        try
        {
            // Check if user is logged in before loading vouchers
            var isLoggedIn = await _authService.IsLoggedInAsync();
            if (!isLoggedIn) return;

            var vouchers = await _apiService.GetAvailableVouchersAsync();
            if (vouchers != null && vouchers.Any())
            {
                AvailableVouchers = new ObservableCollection<VoucherModel>(vouchers);
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Warn("BookingVM", $"⚠️ Failed to load vouchers: {ex.Message}");
        }
    }

    partial void OnCheckInDateChanged(DateTime value)
    {
        UpdatePriceCalculations();
    }

    partial void OnCheckOutDateChanged(DateTime value)
    {
        UpdatePriceCalculations();
    }

    partial void OnPricePerNightChanged(decimal value)
    {
        UpdatePriceCalculations();
    }

    partial void OnSelectedVoucherChanged(VoucherModel? value)
    {
        UpdatePriceCalculations();
    }

    private void UpdatePriceCalculations()
    {
        // Calculate discount from selected voucher
        if (SelectedVoucher != null)
        {
            var subTotal = SubTotal;

            // Check if minimum order value is met
            if (SelectedVoucher.MinOrderValue == null || subTotal >= SelectedVoucher.MinOrderValue)
            {
                Discount = SelectedVoucher.DiscountType == "Percent"
                    ? subTotal * (SelectedVoucher.DiscountValue / 100)
                    : SelectedVoucher.DiscountValue;
            }
            else
            {
                Discount = 0;
            }
        }
        else
        {
            Discount = 0;
        }

        // Notify all properties
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(FinalTotal));
        OnPropertyChanged(nameof(NightsCount));
        OnPropertyChanged(nameof(NightsDisplay));
        OnPropertyChanged(nameof(CheckInDisplay));
        OnPropertyChanged(nameof(CheckOutDisplay));
        OnPropertyChanged(nameof(SubTotalDisplay));
        OnPropertyChanged(nameof(DiscountDisplay));
        OnPropertyChanged(nameof(FinalTotalDisplay));
    }

    [RelayCommand]
    private async Task ConfirmBookingAsync()
    {
        if (IsLoading) return;

        // ✅ Check authentication first
        var isLoggedIn = await _authService.IsLoggedInAsync();
        if (!isLoggedIn)
        {
            var loginConfirmed = await Shell.Current.DisplayAlert(
                "Login Required",
                "You need to login to make a booking. Would you like to login now?",
                "Login", "Cancel");

            if (loginConfirmed)
            {
                await Shell.Current.GoToAsync("//LoginPage");
            }
            return;
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(GuestName))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please enter your name.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(GuestPhone))
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please enter your phone number.", "OK");
            return;
        }

        if (SelectedPaymentMethod == null)
        {
            await Shell.Current.DisplayAlert("Validation Error", "Please select a payment method.", "OK");
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Android.Util.Log.Info("BookingVM", $"🔵 Creating booking - Room: {RoomId}, Check-in: {CheckInDate:yyyy-MM-dd}, Check-out: {CheckOutDate:yyyy-MM-dd}");

            // Get current user ID
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
            {
                await Shell.Current.DisplayAlert("Error", "Please login again", "OK");
                return;
            }

            // Create booking request with userId
            var request = new CreateBookingRequest
            {
                UserId = currentUser.Id, // ✅ Send userId to backend
                RoomId = RoomId,
                CheckIn = CheckInDate,
                CheckOut = CheckOutDate,
                GuestName = GuestName,
                GuestPhone = GuestPhone,
                VoucherCode = SelectedVoucher?.Code, // Include voucher if selected
                PaymentMethod = SelectedPaymentMethod?.Code ?? "COD" // ✅ Send payment method
            };

            Android.Util.Log.Info("BookingVM", $"📤 Sending booking with UserId: {request.UserId}, PaymentMethod: {request.PaymentMethod}");

            var response = await _apiService.CreateBookingAsync(request);

            if (response.Success)
            {
                Android.Util.Log.Info("BookingVM", $"✅ Booking created successfully: {response.BookingId}");

                var successMessage = $"Your booking has been confirmed.\n\n" +
                    $"Booking ID: {response.BookingId}\n" +
                    $"Hotel: {HotelName}\n" +
                    $"Room: {RoomName}\n" +
                    $"Check-in: {CheckInDisplay}\n" +
                    $"Check-out: {CheckOutDisplay}\n\n";

                if (Discount > 0)
                {
                    successMessage += $"Subtotal: {SubTotalDisplay}\n" +
                        $"Discount: {DiscountDisplay}\n" +
                        $"Total: {FinalTotalDisplay}";
                }
                else
                {
                    successMessage += $"Total: {FinalTotalDisplay}";
                }

                // Show success message
                await Shell.Current.DisplayAlert("Booking Confirmed!", successMessage, "OK");

                // Navigate back to hotels page
                await Shell.Current.GoToAsync("//HotelsPage");
            }
            else
            {
                Android.Util.Log.Warn("BookingVM", $"⚠️ Booking failed: {response.Message}");
                ErrorMessage = response.Message ?? "Booking failed. Please try again.";
                await Shell.Current.DisplayAlert("Booking Failed", ErrorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("BookingVM", $"❌ Exception: {ex.Message}");
            ErrorMessage = $"Error creating booking: {ex.Message}";
            await Shell.Current.DisplayAlert("Error", ErrorMessage, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
