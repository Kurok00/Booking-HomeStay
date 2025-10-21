using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;

namespace ChillNest.MobileApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public LoginViewModel(IApiService apiService)
    {
        _apiService = apiService;
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

            var request = new LoginRequest
            {
                Email = Email.Trim(),
                Password = Password
            };

            var response = await _apiService.LoginAsync(request);

            if (response.Success && response.User != null)
            {
                // Save user info (in real app, use SecureStorage)
                await SecureStorage.SetAsync("user_id", response.User.Id.ToString());
                await SecureStorage.SetAsync("user_email", response.User.Email);
                await SecureStorage.SetAsync("user_name", response.User.UserName);

                // Navigate to main page
                await Shell.Current.GoToAsync("///HotelsPage");
            }
            else
            {
                ErrorMessage = response.Message ?? "Login failed";
            }
        }
        catch (Exception ex)
        {
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
        await Shell.Current.GoToAsync("RegisterPage");
    }
}
