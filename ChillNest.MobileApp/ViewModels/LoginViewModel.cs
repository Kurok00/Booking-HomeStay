using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;

namespace ChillNest.MobileApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(IApiService apiService, IAuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsLoading) return;

        // Validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your password";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            Android.Util.Log.Info("LoginVM", $"🔵 Logging in - Email: {Email}");

            var request = new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password
            };

            var response = await _apiService.LoginAsync(request);

            Android.Util.Log.Info("LoginVM", $"Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

            if (response.Success && response.User != null)
            {
                // Save user info using AuthService
                await _authService.SaveUserAsync(response.User);
                // Use userId as token (simple approach)
                await _authService.SaveTokenAsync(response.User.Id.ToString());

                Android.Util.Log.Info("LoginVM", $"✅ Login successful - User: {response.User.UserName}");

                // Navigate to hotels page
                await Shell.Current.GoToAsync("///HotelsPage");
            }
            else
            {
                Android.Util.Log.Warn("LoginVM", $"⚠️ Login failed: {response.Message}");
                ErrorMessage = response.Message ?? "Login failed";
            }
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error("LoginVM", $"❌ Login error: {ex.Message}");
            ErrorMessage = $"Login error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync(nameof(Views.RegisterPage));
    }
}
