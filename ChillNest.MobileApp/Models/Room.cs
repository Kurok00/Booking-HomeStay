using Newtonsoft.Json;

namespace ChillNest.MobileApp.Models;

public class Room
{
    [JsonProperty("roomId")]
    public int RoomId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("capacity")]
    public int Capacity { get; set; }

    [JsonProperty("bedType")]
    public string? BedType { get; set; }

    [JsonProperty("size")]
    public int? Size { get; set; }

    [JsonProperty("status")]
    public bool Status { get; set; }

    [JsonProperty("photos")]
    public List<string>? Photos { get; set; }

    [JsonProperty("amenities")]
    public List<RoomAmenity>? Amenities { get; set; }

    // Display properties
    public string PriceDisplay => $"${Price:F0}/night";
    public string CapacityDisplay => $"{Capacity} guests";
    public string BedTypeDisplay => BedType ?? "Standard bed";
    public string SizeDisplay => Size.HasValue ? $"{Size}m²" : "";
    public string StatusDisplay => Status ? "Available" : "Not Available";

    public string MainPhotoUrl
    {
        get
        {
            var photo = Photos?.FirstOrDefault();
            if (string.IsNullOrEmpty(photo))
                return "https://via.placeholder.com/300x200";

            return !photo.StartsWith("http")
                ? $"http://10.0.2.2:5182{photo}"
                : photo;
        }
    }
}

public class RoomAmenity
{
    [JsonProperty("amenityId")]
    public int AmenityId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("icon")]
    public string? Icon { get; set; }
}
