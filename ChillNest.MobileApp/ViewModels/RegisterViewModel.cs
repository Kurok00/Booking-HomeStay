using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChillNest.MobileApp.Models;
using ChillNest.MobileApp.Services;

namespace ChillNest.MobileApp.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string fullName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public RegisterViewModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsLoading) return;

        // Validation
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Please enter your email";
            return;
        }

        if (string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = "Please enter a username";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter a password";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match";
            return;
        }

        if (Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var request = new RegisterRequest
            {
                Email = Email.Trim(),
                UserName = UserName.Trim(),
                Password = Password,
                FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim()
            };

            var response = await _apiService.RegisterAsync(request);

            if (response.Success)
            {
                await Shell.Current.DisplayAlert("Success", response.Message, "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = response.Message ?? "Registration failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Registration error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
