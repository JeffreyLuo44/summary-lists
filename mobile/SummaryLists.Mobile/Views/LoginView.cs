using SummaryLists.Mobile.ViewModels;
using SummaryLists.Mobile.Extensions;

namespace SummaryLists.Mobile.Views;

public sealed class LoginView : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginView(LoginViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;

        Title = "Sign In";

        var titleLabel = new Label()
            .Text("Summary Lists")
            .FontSize(30)
            .Bold();

        var modeHeadingLabel = new Label()
            .FontSize(20)
            .Bold()
            .Bind(Label.TextProperty, nameof(LoginViewModel.ModeHeading));

        var subtitleLabel = new Label()
            .Text("Sign in or register to access your lists.");

        var emailEntry = new Entry()
            .Placeholder("Email")
            .Keyboard(Keyboard.Email)
            .Bind(Entry.TextProperty, nameof(LoginViewModel.Email), BindingMode.TwoWay);

        var passwordEntry = new Entry()
            .Placeholder("Password")
            .Password()
            .Bind(Entry.TextProperty, nameof(LoginViewModel.Password), BindingMode.TwoWay);

        var confirmPasswordEntry = new Entry()
            .Placeholder("Confirm password")
            .Password()
            .Bind(Entry.TextProperty, nameof(LoginViewModel.ConfirmPassword), BindingMode.TwoWay)
            .Bind(IsVisibleProperty, nameof(LoginViewModel.ShowConfirmPassword));

        var submitButton = new Button()
            .Bind(Button.TextProperty, nameof(LoginViewModel.PrimaryActionText))
            .Bind(Button.CommandProperty, nameof(LoginViewModel.SubmitCommand))
            .Bind(IsEnabledProperty, nameof(LoginViewModel.IsNotBusy));

        var toggleModeButton = new Button()
            .Bind(Button.TextProperty, nameof(LoginViewModel.ToggleModeText))
            .Bind(Button.CommandProperty, nameof(LoginViewModel.ToggleModeCommand))
            .Bind(IsEnabledProperty, nameof(LoginViewModel.IsNotBusy));

        var busyIndicator = new ActivityIndicator()
            .Bind(IsVisibleProperty, nameof(LoginViewModel.IsBusy))
            .Bind(ActivityIndicator.IsRunningProperty, nameof(LoginViewModel.IsBusy));

        var errorLabel = new Label()
            .TextColor(Color.FromArgb("#B00020"))
            .Bind(Label.TextProperty, nameof(LoginViewModel.ErrorMessage));

        var bodyLayout = new VerticalStackLayout
        {
            Children =
            {
                titleLabel,
                modeHeadingLabel,
                subtitleLabel,
                emailEntry,
                passwordEntry,
                confirmPasswordEntry,
                submitButton,
                toggleModeButton,
                busyIndicator,
                errorLabel,
            },
        }
        .Padding(20)
        .Spacing(12);

        Content = new ScrollView
        {
            Content = bodyLayout,
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
