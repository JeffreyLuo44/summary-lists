using SummaryLists.Mobile.Tests.TestDoubles;
using SummaryLists.Mobile.Services;
using SummaryLists.Mobile.ViewModels;

namespace SummaryLists.Mobile.Tests.ViewModels;

public class LoginViewModelTests
{
    [Fact]
    public async Task Initialize_WhenSessionExists_NavigatesToLists()
    {
        var auth = new FakeAuthService
        {
            Session = new AuthSession
            {
                IdToken = "token",
                Email = "user@example.com",
            },
        };
        var navigation = new FakeNavigationService();
        var viewModel = new LoginViewModel(auth, navigation);

        await viewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal(1, navigation.NavigateToListsCalls);
    }

    [Fact]
    public async Task Submit_RegisterMode_WithMismatchedPasswords_ShowsError()
    {
        var auth = new FakeAuthService();
        var navigation = new FakeNavigationService();
        var viewModel = new LoginViewModel(auth, navigation)
        {
            IsRegisterMode = true,
            Email = "user@example.com",
            Password = "abcdef",
            ConfirmPassword = "different",
        };

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.Equal("Passwords do not match.", viewModel.ErrorMessage);
        Assert.Equal(0, auth.RegisterCalls);
        Assert.Equal(0, navigation.NavigateToListsCalls);
    }

    [Fact]
    public async Task Submit_RegisterMode_ValidInput_RegistersAndNavigates()
    {
        var auth = new FakeAuthService();
        var navigation = new FakeNavigationService();
        var viewModel = new LoginViewModel(auth, navigation)
        {
            IsRegisterMode = true,
            Email = "user@example.com",
            Password = "abcdef",
            ConfirmPassword = "abcdef",
        };

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(1, auth.RegisterCalls);
        Assert.Equal(1, navigation.NavigateToListsCalls);
        Assert.Equal(string.Empty, viewModel.Password);
        Assert.Equal(string.Empty, viewModel.ConfirmPassword);
    }

    [Fact]
    public async Task Submit_LoginMode_ValidInput_SignsInAndNavigates()
    {
        var auth = new FakeAuthService();
        var navigation = new FakeNavigationService();
        var viewModel = new LoginViewModel(auth, navigation)
        {
            IsRegisterMode = false,
            Email = "user@example.com",
            Password = "abcdef",
        };

        await viewModel.SubmitCommand.ExecuteAsync(null);

        Assert.Equal(1, auth.SignInCalls);
        Assert.Equal(1, navigation.NavigateToListsCalls);
    }

    [Fact]
    public void ToggleMode_UpdatesModeDependentProperties()
    {
        var auth = new FakeAuthService();
        var navigation = new FakeNavigationService();
        var viewModel = new LoginViewModel(auth, navigation);

        viewModel.ToggleModeCommand.Execute(null);

        Assert.True(viewModel.IsRegisterMode);
        Assert.True(viewModel.ShowConfirmPassword);
        Assert.Equal("Create Account", viewModel.ModeHeading);
    }
}
