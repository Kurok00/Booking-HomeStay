using ChillNest.MobileApp.Models;

namespace ChillNest.MobileApp.Services;

public interface IAuthService
{
    Task<bool> IsLoggedInAsync();
    Task<string?> GetTokenAsync();
    Task SaveTokenAsync(string token);
    Task<User?> GetCurrentUserAsync();
    Task SaveUserAsync(User user);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private const string TokenKey = "auth_token";
    private const string UserKey = "current_user";

    public async Task<bool> IsLoggedInAsync()
    {
        var token = await SecureStorage.GetAsync(TokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(TokenKey);
    }

    public async Task SaveTokenAsync(string token)
    {
        await SecureStorage.SetAsync(TokenKey, token);
#if ANDROID
        Android.Util.Log.Info("AuthService", $"✅ Token saved");
#endif
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var json = await SecureStorage.GetAsync(UserKey);
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<User>(json);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveUserAsync(User user)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);
        await SecureStorage.SetAsync(UserKey, json);
#if ANDROID
        Android.Util.Log.Info("AuthService", $"✅ User saved: {user.UserName}");
#endif
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(UserKey);
        await Task.CompletedTask;
#if ANDROID
        Android.Util.Log.Info("AuthService", "✅ Logged out");
#endif
    }
}
