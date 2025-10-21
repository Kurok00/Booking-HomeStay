using ChillNest.MobileApp.ViewModels;

namespace ChillNest.MobileApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}