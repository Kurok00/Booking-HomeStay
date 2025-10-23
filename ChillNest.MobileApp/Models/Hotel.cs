using Newtonsoft.Json;

namespace ChillNest.MobileApp.Models;

public class Hotel
{
    [JsonProperty("hotelId")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string? Address { get; set; }

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("country")]
    public string? Country { get; set; }

    [JsonProperty("minPrice")]
    public decimal? PricePerNight { get; set; }

    [JsonProperty("avgRating")]
    public double? Rating { get; set; }

    [JsonProperty("latitude")]
    public double? Latitude { get; set; }

    [JsonProperty("longitude")]
    public double? Longitude { get; set; }

    [JsonProperty("mainPhoto")]
    public string? MainPhoto { get; set; }

    [JsonProperty("photos")]
    public List<string>? PhotoUrls { get; set; }

    [JsonProperty("roomCount")]
    public int RoomCount { get; set; }

    public bool IsVisible { get; set; }
    public List<Room>? Rooms { get; set; }
    public List<Review>? Reviews { get; set; }
    public List<Amenity>? Amenities { get; set; }

    // Display properties
    public string MainPhotoUrl
    {
        get
        {
            // Try MainPhoto first, then PhotoUrls[0], then placeholder
            var photo = MainPhoto ?? (PhotoUrls?.FirstOrDefault());
            if (string.IsNullOrEmpty(photo))
                return "https://via.placeholder.com/300x200";

            return !photo.StartsWith("http")
                ? $"http://10.0.2.2:5182{photo}"
                : photo;
        }
    }
    public string RatingDisplay => Rating.HasValue ? $"{Rating:F1} ⭐" : "No rating";
    public string PriceDisplay => PricePerNight.HasValue ? $"${PricePerNight:F0}/night" : "Price not available";
    public string LocationDisplay => !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(Country)
        ? $"{City}, {Country}"
        : Address ?? "Location not specified";
}

public class Photo
{
    public int PhotoId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public int HotelId { get; set; }
}

public class Amenity
{
    public int AmenitiesId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public string? Description { get; set; }
}
