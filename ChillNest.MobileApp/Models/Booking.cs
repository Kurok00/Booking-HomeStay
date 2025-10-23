using Newtonsoft.Json;

namespace ChillNest.MobileApp.Models;

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public decimal Total { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Room? Room { get; set; }
    public Hotel? Hotel { get; set; }

    // Display properties
    public string StatusDisplay => Status.ToString();
    public string TotalDisplay => $"${Total:F2}";
    public string DateRangeDisplay => $"{CheckInDate:MMM dd} - {CheckOutDate:MMM dd, yyyy}";
    public int NightsCount => (CheckOutDate - CheckInDate).Days;
    public string NightsDisplay => $"{NightsCount} {(NightsCount == 1 ? "night" : "nights")}";
    public string StatusColor => Status switch
    {
        BookingStatus.Pending => "#FFA500", // Orange
        BookingStatus.Confirmed => "#4CAF50", // Green
        BookingStatus.Paid => "#2196F3", // Blue
        BookingStatus.Canceled => "#F44336", // Red
        _ => "#9E9E9E" // Gray
    };
}

public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Paid = 2,
    Canceled = 3
}

public class CreateBookingRequest
{
    [JsonProperty("userId")]
    public int UserId { get; set; } // ✅ Send userId to backend

    [JsonProperty("roomId")]
    public int RoomId { get; set; }

    [JsonProperty("checkIn")]
    public DateTime CheckIn { get; set; }

    [JsonProperty("checkOut")]
    public DateTime CheckOut { get; set; }

    [JsonProperty("guestName")]
    public string GuestName { get; set; } = string.Empty;

    [JsonProperty("guestPhone")]
    public string GuestPhone { get; set; } = string.Empty;

    [JsonProperty("voucherCode")]
    public string? VoucherCode { get; set; } // ✅ Voucher support

    [JsonProperty("paymentMethod")]
    public string PaymentMethod { get; set; } = "COD"; // ✅ Payment method: COD or ONLINE
}

public class BookingResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int BookingId { get; set; }
    public Booking? Booking { get; set; }
}

public class PaymentMethod
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
}

// API response model for bookings list
public class BookingApiResponse
{
    [JsonProperty("bookingId")]
    public int BookingId { get; set; }

    [JsonProperty("checkIn")]
    public DateTime CheckIn { get; set; }

    [JsonProperty("checkOut")]
    public DateTime CheckOut { get; set; }

    [JsonProperty("totalPrice")]
    public decimal TotalPrice { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("room")]
    public RoomInfo? Room { get; set; }

    [JsonProperty("hotel")]
    public HotelInfo? Hotel { get; set; }

    public class RoomInfo
    {
        [JsonProperty("roomId")]
        public int RoomId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("mainPhoto")]
        public string MainPhoto { get; set; } = string.Empty;
    }

    public class HotelInfo
    {
        [JsonProperty("hotelId")]
        public int HotelId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("address")]
        public string Address { get; set; } = string.Empty;

        [JsonProperty("city")]
        public string City { get; set; } = string.Empty;
    }
}

// Model for booking list item
public class BookingItem
{
    [JsonProperty("bookingId")]
    public int BookingId { get; set; }

    [JsonProperty("hotelName")]
    public string HotelName { get; set; } = string.Empty;

    [JsonProperty("roomName")]
    public string RoomName { get; set; } = string.Empty;

    [JsonProperty("roomPhoto")]
    public string RoomPhoto { get; set; } = string.Empty;

    [JsonProperty("checkIn")]
    public DateTime CheckIn { get; set; }

    [JsonProperty("checkOut")]
    public DateTime CheckOut { get; set; }

    [JsonProperty("totalPrice")]
    public decimal TotalPrice { get; set; }

    [JsonProperty("status")]
    public string StatusString { get; set; } = "Pending";

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    // Computed properties
    public BookingStatus Status => Enum.TryParse<BookingStatus>(StatusString, out var status) ? status : BookingStatus.Pending;
    public string CheckInDisplay => CheckIn.ToString("MMM dd, yyyy");
    public string CheckOutDisplay => CheckOut.ToString("MMM dd, yyyy");
    public string DateRangeDisplay => $"{CheckIn:MMM dd} - {CheckOut:MMM dd, yyyy}";
    public int NightsCount => (CheckOut - CheckIn).Days;
    public string NightsDisplay => $"{NightsCount} {(NightsCount == 1 ? "night" : "nights")}";
    public string TotalDisplay => $"${TotalPrice:F0}";
    public string StatusDisplay => Status.ToString();
    public string StatusColor => Status switch
    {
        BookingStatus.Pending => "#FFA500",    // Orange
        BookingStatus.Confirmed => "#2196F3",  // Blue
        BookingStatus.Paid => "#4CAF50",       // Green
        BookingStatus.Canceled => "#F44336",   // Red
        _ => "#9E9E9E"                         // Gray
    };
    public bool CanCancel => Status == BookingStatus.Pending || Status == BookingStatus.Confirmed;
}
