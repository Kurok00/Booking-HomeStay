namespace ChillNest.MobileApp.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public decimal PricePerNight { get; set; }
    public int Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
    public List<RoomAmenity>? RoomAmenities { get; set; }
    public List<Photo>? Photos { get; set; }

    // Display properties
    public string PriceDisplay => $"${PricePerNight:F0}/night";
    public string CapacityDisplay => $"{Capacity} guests";
    public string AvailabilityDisplay => IsAvailable ? "Available" : "Not Available";
    public string MainPhotoUrl => Photos?.FirstOrDefault()?.PhotoUrl ?? "https://via.placeholder.com/300x200";
}

public class RoomAmenity
{
    public int RoomAmenitieId { get; set; }
    public int RoomId { get; set; }
    public int AmenitiesId { get; set; }
    public Amenity? Amenitie { get; set; }
}
