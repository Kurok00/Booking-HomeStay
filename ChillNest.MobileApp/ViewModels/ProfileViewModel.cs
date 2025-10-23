using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;
using ChillNest.MobileApp.Views;

namespace ChillNest.MobileApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private User? currentUser;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string avatarUrl = string.Empty;

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isLoggedIn;

    public ProfileViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        await LoadProfileAsync();
    }

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        try
        {
            IsLoading = true;

            Android.Util.Log.Info("ProfileVM", "🔵 LoadProfileAsync called");

            // Check if logged in
            IsLoggedIn = await _authService.IsLoggedInAsync();
            Android.Util.Log.Info("ProfileVM", $"IsLoggedIn: {IsLoggedIn}");

            if (!IsLoggedIn)
            {
                Android.Util.Log.Info("ProfileVM", "⚠️ User not logged in");
                CurrentUser = null;
                UserName = string.Empty;
                FullName = string.Empty;
                Email = string.Empty;
                Phone = string.Empty;
                AvatarUrl = string.Empty;
                return;
            }

            // Get profile from API
            Android.Util.Log.Info("ProfileVM", "📡 Fetching profile from API...");
            var user = await _apiService.GetProfileAsync();

            if (user != null)
            {
                Android.Util.Log.Info("ProfileVM", $"✅ Profile loaded from API: {user.UserName}");
                CurrentUser = user;
                UserName = user.UserName;
                FullName = user.FullName ?? "";
                Email = user.Email;
                Phone = user.Phone ?? "";
                AvatarUrl = user.AvatarDisplay;

                // Update local storage
                await _authService.SaveUserAsync(user);
            }
            else
            {
                Android.Util.Log.Warn("ProfileVM", "⚠️ API returned null, trying local storage...");

                // Fallback to local storage
                var localUser = await _authService.GetCurrentUserAsync();
                if (localUser != null)
                {
                    Android.Util.Log.Info("ProfileVM", $"✅ Profile loaded from local storage: {localUser.UserName}");
                    CurrentUser = localUser;
                    UserName = localUser.UserName;
                    FullName = localUser.FullName ?? "";
                    Email = localUser.Email;
                    Phone = localUser.Phone ?? "";
                    AvatarUrl = localUser.AvatarDisplay;
                    IsLoggedIn = true; // Update IsLoggedIn since we have user data
                }
                else
                {
                    Android.Util.Log.Error("ProfileVM", "❌ No user data in local storage");
                    IsLoggedIn = false;
                }
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("ProfileVM", $"❌ Failed to load profile: {ex.Message}");
            Android.Util.Log.Error("ProfileVM", $"Stack trace: {ex.StackTrace}");

            // Fallback to local storage
            var localUser = await _authService.GetCurrentUserAsync();
            if (localUser != null)
            {
                Android.Util.Log.Info("ProfileVM", $"✅ Fallback: Profile loaded from local storage: {localUser.UserName}");
                CurrentUser = localUser;
                UserName = localUser.UserName;
                FullName = localUser.FullName ?? "";
                Email = localUser.Email;
                Phone = localUser.Phone ?? "";
                AvatarUrl = localUser.AvatarDisplay;
                IsLoggedIn = true; // Update IsLoggedIn since we have user data
            }
            else
            {
                Android.Util.Log.Error("ProfileVM", "❌ Fallback failed: No local user data");
                IsLoggedIn = false;
            }
        }
        finally
        {
            IsLoading = false;
            Android.Util.Log.Info("ProfileVM", $"🏁 LoadProfileAsync completed. IsLoggedIn: {IsLoggedIn}");
        }
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        if (!IsEditing)
        {
            // Enter edit mode
            IsEditing = true;
        }
        else
        {
            // Cancel edit - restore original values
            if (CurrentUser != null)
            {
                UserName = CurrentUser.UserName;
                FullName = CurrentUser.FullName ?? "";
                Phone = CurrentUser.Phone ?? "";
            }
            IsEditing = false;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (!IsEditing || IsLoading) return;

        try
        {
            IsLoading = true;

            var request = new UpdateProfileRequest
            {
                UserName = UserName,
                FullName = FullName,
                Phone = Phone
            };

            var response = await _apiService.UpdateProfileAsync(request);

            if (response.Success)
            {
                // Update current user
                if (CurrentUser != null)
                {
                    CurrentUser.UserName = UserName;
                    CurrentUser.FullName = FullName;
                    CurrentUser.Phone = Phone;

                    // Update local storage
                    await _authService.SaveUserAsync(CurrentUser);
                }

                IsEditing = false;

                await Shell.Current.DisplayAlert(
                    "Success",
                    "Profile updated successfully!",
                    "OK"
                );
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Error",
                    response.Message ?? "Failed to update profile",
                    "OK"
                );
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("ProfileVM", $"❌ Failed to save profile: {ex.Message}");
            await Shell.Current.DisplayAlert(
                "Error",
                $"Failed to update profile: {ex.Message}",
                "OK"
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes", "Cancel"
        );

        if (!confirmed) return;

        try
        {
            IsLoading = true;

            // Call API logout (if exists)
            await _apiService.LogoutAsync();

            // Clear local storage
            await _authService.LogoutAsync();

            // Reset state
            CurrentUser = null;
            IsLoggedIn = false;
            IsEditing = false;
            UserName = "";
            FullName = "";
            Email = "";
            Phone = "";
            AvatarUrl = "";

            await Shell.Current.DisplayAlert(
                "Logged Out",
                "You have been logged out successfully",
                "OK"
            );

            // Navigate to login page
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("ProfileVM", $"❌ Logout failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}
