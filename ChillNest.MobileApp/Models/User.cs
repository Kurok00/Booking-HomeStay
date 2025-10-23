using Newtonsoft.Json;

namespace ChillNest.MobileApp.Models;

public class User
{
    [JsonProperty("userId")]
    public int Id { get; set; }

    [JsonProperty("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;

    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [JsonProperty("fullName")]
    public string? FullName { get; set; }

    [JsonProperty("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonProperty("roles")]
    public List<string> Roles { get; set; } = new();

    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }

    // Display properties
    public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : UserName;
    public string InitialsDisplay => GetInitials();
    public string AvatarDisplay => !string.IsNullOrEmpty(AvatarUrl) && !AvatarUrl.StartsWith("http")
        ? $"http://10.0.2.2:5182{AvatarUrl}"
        : AvatarUrl ?? "https://via.placeholder.com/150";

    private string GetInitials()
    {
        if (!string.IsNullOrEmpty(FullName))
        {
            var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpper();
        }
        return UserName.Length > 0 ? UserName[0].ToString().ToUpper() : "?";
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? FullName { get; set; }
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [JsonProperty("userName")]
    public string? UserName { get; set; }

    [JsonProperty("fullName")]
    public string? FullName { get; set; }

    [JsonProperty("phone")]
    public string? Phone { get; set; }
}

public class UpdateProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public User? User { get; set; }
}
