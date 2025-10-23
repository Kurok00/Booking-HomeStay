using Newtonsoft.Json;

namespace ChillNest.MobileApp.Models;

public class Review
{
    [JsonProperty("reviewId")]
    public int ReviewId { get; set; }

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("comment")]
    public string? Comment { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("user")]
    public ReviewUser? User { get; set; }

    [JsonProperty("photos")]
    public List<string>? Photos { get; set; }

    // Display properties
    public string RatingDisplay => $"{Rating} ⭐";
    public string DateDisplay => CreatedAt.ToString("MMM dd, yyyy");
    public string UserNameDisplay => User?.UserName ?? "Anonymous";
    public string UserAvatarUrl
    {
        get
        {
            var avatar = User?.AvatarUrl;
            if (string.IsNullOrEmpty(avatar))
                return "https://via.placeholder.com/50";

            return !avatar.StartsWith("http")
                ? $"http://10.0.2.2:5182{avatar}"
                : avatar;
        }
    }
}

public class ReviewUser
{
    [JsonProperty("userId")]
    public int UserId { get; set; }

    [JsonProperty("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonProperty("avatarUrl")]
    public string? AvatarUrl { get; set; }
}
