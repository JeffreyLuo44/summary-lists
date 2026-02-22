using SummaryLists.Mobile.Models;
using SummaryLists.Mobile.Services;
using SummaryLists.Mobile.Tests.TestDoubles;
using SummaryLists.Mobile.ViewModels;

namespace SummaryLists.Mobile.Tests.ViewModels;

public class ListsViewModelTests
{
    [Fact]
    public async Task Initialize_WithoutSession_NavigatesToLogin()
    {
        var auth = new FakeAuthService { Session = null };
        var api = new FakeApiClient();
        var navigation = new FakeNavigationService();
        var dialog = new FakeDialogService();
        var viewModel = new ListsViewModel(api, auth, navigation, dialog);

        await viewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal(1, navigation.NavigateToLoginCalls);
        Assert.Empty(viewModel.Lists);
    }

    [Fact]
    public async Task Initialize_WithSession_LoadsListsAndItems()
    {
        var auth = new FakeAuthService
        {
            Session = new AuthSession { IdToken = "token", Email = "user@example.com" },
        };
        var api = new FakeApiClient();
        api.SetLists(
            new ListModel
            {
                Id = "older",
                UserId = "user-1",
                Title = "Older",
                Summary = "older summary",
                CreatedAt = "2026-01-01T00:00:00.0000000Z",
                UpdatedAt = "2026-01-01T00:00:00.0000000Z",
            },
            new ListModel
            {
                Id = "newer",
                UserId = "user-1",
                Title = "Newer",
                Summary = "newer summary",
                CreatedAt = "2026-01-02T00:00:00.0000000Z",
                UpdatedAt = "2026-01-02T00:00:00.0000000Z",
            });
        api.SetItems(
            "newer",
            new ListItemModel
            {
                Id = "item-1",
                ListId = "newer",
                UserId = "user-1",
                Text = "Item",
                Note = "Note",
                Position = 1,
                CreatedAt = "2026-01-02T00:00:00.0000000Z",
                UpdatedAt = "2026-01-02T00:00:00.0000000Z",
            });

        var navigation = new FakeNavigationService();
        var dialog = new FakeDialogService();
        var viewModel = new ListsViewModel(api, auth, navigation, dialog);

        await viewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal("user@example.com", viewModel.SessionEmail);
        Assert.Equal(2, viewModel.Lists.Count);
        Assert.Equal("newer", viewModel.SelectedList?.Id);
        Assert.Single(viewModel.Items);
        Assert.Equal("Item", viewModel.Items[0].Text);
    }

    [Fact]
    public async Task AddList_ValidTitle_AddsAndSelectsNewList()
    {
        var auth = new FakeAuthService
        {
            Session = new AuthSession { IdToken = "token", Email = "user@example.com" },
        };
        var api = new FakeApiClient();
        var navigation = new FakeNavigationService();
        var dialog = new FakeDialogService();
        var viewModel = new ListsViewModel(api, auth, navigation, dialog);

        await viewModel.InitializeCommand.ExecuteAsync(null);
        viewModel.NewListTitle = "Groceries";

        await viewModel.AddListCommand.ExecuteAsync(null);

        Assert.Single(viewModel.Lists);
        Assert.Equal("Groceries", viewModel.SelectedList?.Title);
        Assert.Equal(string.Empty, viewModel.NewListTitle);
    }

    [Fact]
    public async Task AddItem_WithEmptyText_SetsError()
    {
        var auth = new FakeAuthService
        {
            Session = new AuthSession { IdToken = "token", Email = "user@example.com" },
        };
        var api = new FakeApiClient();
        api.SetLists(
            new ListModel
            {
                Id = "list-1",
                UserId = "user-1",
                Title = "My List",
                Summary = "summary",
                CreatedAt = "2026-01-01T00:00:00.0000000Z",
                UpdatedAt = "2026-01-01T00:00:00.0000000Z",
            });

        var navigation = new FakeNavigationService();
        var dialog = new FakeDialogService();
        var viewModel = new ListsViewModel(api, auth, navigation, dialog);

        await viewModel.InitializeCommand.ExecuteAsync(null);
        viewModel.NewItemText = " ";

        await viewModel.AddItemCommand.ExecuteAsync(null);

        Assert.Equal("Item text is required.", viewModel.ErrorMessage);
        Assert.Empty(viewModel.Items);
    }

    [Fact]
    public async Task DeleteList_WhenConfirmed_RemovesList()
    {
        var auth = new FakeAuthService
        {
            Session = new AuthSession { IdToken = "token", Email = "user@example.com" },
        };
        var api = new FakeApiClient();
        api.SetLists(
            new ListModel
            {
                Id = "list-1",
                UserId = "user-1",
                Title = "A",
                Summary = "summary",
                CreatedAt = "2026-01-01T00:00:00.0000000Z",
                UpdatedAt = "2026-01-01T00:00:00.0000000Z",
            });
        var navigation = new FakeNavigationService();
        var dialog = new FakeDialogService { ConfirmResult = true };
        var viewModel = new ListsViewModel(api, auth, navigation, dialog);
        await viewModel.InitializeCommand.ExecuteAsync(null);

        await viewModel.DeleteListCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.Lists);
        Assert.Null(viewModel.SelectedList);
    }

    [Fact]
    public async Task Logout_SignsOutAndNavigatesToLogin()
    {
        var auth = new FakeAuthService
        {
            Session = new AuthSession { IdToken = "token", Email = "user@example.com" },
        };
        var api = new FakeApiClient();
        var navigation = new FakeNavigationService();
        var dialog = new FakeDialogService();
        var viewModel = new ListsViewModel(api, auth, navigation, dialog);

        await viewModel.LogoutCommand.ExecuteAsync(null);

        Assert.Equal(1, auth.SignOutCalls);
        Assert.Equal(1, navigation.NavigateToLoginCalls);
    }
}
