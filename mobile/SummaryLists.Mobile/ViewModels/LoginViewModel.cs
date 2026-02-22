using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SummaryLists.Mobile.Services;

namespace SummaryLists.Mobile.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private bool isRegisterMode;

    public string ModeHeading => IsRegisterMode ? "Create Account" : "Sign In";

    public string PrimaryActionText => IsRegisterMode ? "Create Account" : "Log In";

    public string ToggleModeText => IsRegisterMode ? "Already have an account? Sign in" : "Need an account? Register";

    public bool ShowConfirmPassword => IsRegisterMode;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;
    }

    partial void OnIsRegisterModeChanged(bool value)
    {
        OnPropertyChanged(nameof(ModeHeading));
        OnPropertyChanged(nameof(PrimaryActionText));
        OnPropertyChanged(nameof(ToggleModeText));
        OnPropertyChanged(nameof(ShowConfirmPassword));
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegisterMode = !IsRegisterMode;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        var session = await _authService.GetSessionAsync();
        if (session is not null && !string.IsNullOrWhiteSpace(session.IdToken))
        {
            await _navigationService.NavigateToListsAsync();
        }
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        await RunBusyAsync(async () =>
        {
            var normalizedEmail = Email.Trim();
            var pwd = Password;
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                throw new InvalidOperationException("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(pwd))
            {
                throw new InvalidOperationException("Password is required.");
            }

            if (pwd.Length < 6)
            {
                throw new InvalidOperationException("Password must be at least 6 characters.");
            }

            if (IsRegisterMode)
            {
                if (!string.Equals(pwd, ConfirmPassword, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Passwords do not match.");
                }

                await _authService.RegisterAsync(normalizedEmail, pwd);
            }
            else
            {
                await _authService.SignInAsync(normalizedEmail, pwd);
            }

            Password = string.Empty;
            ConfirmPassword = string.Empty;
            await _navigationService.NavigateToListsAsync();
        });
    }
}
