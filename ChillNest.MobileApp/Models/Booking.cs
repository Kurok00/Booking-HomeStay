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
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
}

public class BookingResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Booking? Booking { get; set; }
}
