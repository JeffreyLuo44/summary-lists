using Microsoft.Extensions.DependencyInjection;
using SummaryLists.Mobile.Views;

namespace SummaryLists.Mobile.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task NavigateToLoginAsync()
    {
        return SetRootPageAsync<LoginView>();
    }

    public Task NavigateToListsAsync()
    {
        return SetRootPageAsync<ListsView>();
    }

    private Task SetRootPageAsync<TView>() where TView : Page
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is null)
        {
            return Task.CompletedTask;
        }

        var page = _serviceProvider.GetRequiredService<TView>();
        window.Page = new NavigationPage(page);
        return Task.CompletedTask;
    }
}
