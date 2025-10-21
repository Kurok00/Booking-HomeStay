namespace ChillNest.MobileApp.Models;

public class Review
{
    public int ReviewId { get; set; }
    public int HotelId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public User? User { get; set; }
    public List<ReviewPhoto>? ReviewPhotos { get; set; }

    // Display properties
    public string RatingDisplay => $"{Rating} ⭐";
    public string DateDisplay => CreatedAt.ToString("MMM dd, yyyy");
    public string UserNameDisplay => User?.UserName ?? "Anonymous";
}

public class ReviewPhoto
{
    public int PhotoId { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public int ReviewId { get; set; }
}
