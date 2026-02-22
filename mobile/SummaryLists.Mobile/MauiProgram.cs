using Microsoft.Extensions.Logging;
using SummaryLists.Mobile.Services;
using SummaryLists.Mobile.ViewModels;
using SummaryLists.Mobile.Views;

namespace SummaryLists.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<IAuthService, FirebaseAuthService>();
        builder.Services.AddSingleton<IApiClient, ApiClient>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<ListsViewModel>();

        builder.Services.AddTransient<LoginView>();
        builder.Services.AddTransient<ListsView>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
