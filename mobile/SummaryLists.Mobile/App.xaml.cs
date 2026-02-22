using SummaryLists.Mobile.Views;

namespace SummaryLists.Mobile;

public partial class App : Application
{
    private readonly LoginView _loginView;

    public App(LoginView loginView)
    {
        InitializeComponent();
        _loginView = loginView;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new NavigationPage(_loginView));
    }
}
