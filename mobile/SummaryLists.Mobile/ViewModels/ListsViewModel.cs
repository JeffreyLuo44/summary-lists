using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SummaryLists.Mobile.Models;
using SummaryLists.Mobile.Services;

namespace SummaryLists.Mobile.ViewModels;

public partial class ListsViewModel : ViewModelBase
{
    private readonly IApiClient _apiClient;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private bool _suppressSelectionLoad;

    public ObservableCollection<ListModel> Lists { get; } = [];

    public ObservableCollection<ListItemModel> Items { get; } = [];

    [ObservableProperty]
    private string sessionEmail = string.Empty;

    [ObservableProperty]
    private string newListTitle = string.Empty;

    [ObservableProperty]
    private string renameListTitle = string.Empty;

    [ObservableProperty]
    private string newItemText = string.Empty;

    [ObservableProperty]
    private string newItemNote = string.Empty;

    [ObservableProperty]
    private string summaryTitle = "No list selected";

    [ObservableProperty]
    private string summaryText = "Create a list to get started.";

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private bool hasSelectedList;

    [ObservableProperty]
    private ListModel? selectedList;

    public ListsViewModel(
        IApiClient apiClient,
        IAuthService authService,
        INavigationService navigationService,
        IDialogService dialogService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _navigationService = navigationService;
        _dialogService = dialogService;
    }

    partial void OnSelectedListChanged(ListModel? value)
    {
        HasSelectedList = value is not null;
        RenameListTitle = value?.Title ?? string.Empty;
        RenderSummary(value);
        var selectedId = value?.Id;
        foreach (var list in Lists)
        {
            list.IsSelected = selectedId is not null && list.Id == selectedId;
        }
        if (_suppressSelectionLoad)
        {
            return;
        }

        _ = LoadSelectedListItemsSafeAsync();
    }

    [RelayCommand]
    private void SelectList(ListModel? list)
    {
        if (list is null || SelectedList?.Id == list.Id)
        {
            return;
        }

        SelectedList = list;
    }
    [RelayCommand]
    private async Task InitializeAsync()
    {
        await RunBusyAsync(async () =>
        {
            var session = await _authService.GetSessionAsync();
            if (session is null || string.IsNullOrWhiteSpace(session.IdToken))
            {
                await _navigationService.NavigateToLoginAsync();
                return;
            }

            SessionEmail = session.Email;
            await LoadListsCoreAsync();
        });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsRefreshing = true;
        try
        {
            await InitializeAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task AddListAsync()
    {
        var title = NewListTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = "List title is required.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var created = await _apiClient.CreateListAsync(title);
            Lists.Insert(0, created);
            NewListTitle = string.Empty;
            SelectedList = created;
            await LoadSelectedListItemsCoreAsync();
        });
    }

    [RelayCommand]
    private async Task RenameListAsync()
    {
        if (SelectedList is null)
        {
            return;
        }

        var title = RenameListTitle.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            ErrorMessage = "Title is required.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var updated = await _apiClient.UpdateListAsync(SelectedList.Id, title);
            ReplaceList(updated);
            SelectedList = updated;
            ReorderListsByUpdatedAt();
        });
    }

    [RelayCommand]
    private async Task DeleteListAsync()
    {
        if (SelectedList is null)
        {
            return;
        }

        var toDelete = SelectedList;
        var confirmed = await _dialogService.ConfirmAsync(
            "Delete List",
            $"Delete '{toDelete.Title}'?",
            "Delete",
            "Cancel");
        if (!confirmed)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _apiClient.DeleteListAsync(toDelete.Id);
            Lists.Remove(toDelete);
            SelectedList = Lists.FirstOrDefault();
            await LoadSelectedListItemsCoreAsync();
        });
    }

    [RelayCommand]
    private async Task RegenerateSummaryAsync()
    {
        if (SelectedList is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var updated = await _apiClient.RegenerateSummaryAsync(SelectedList.Id);
            ReplaceList(updated);
            SelectedList = updated;
            ReorderListsByUpdatedAt();
        });
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (SelectedList is null)
        {
            return;
        }

        var text = NewItemText.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            ErrorMessage = "Item text is required.";
            return;
        }

        await RunBusyAsync(async () =>
        {
            var note = NewItemNote.Trim();
            var position = Items.Any() ? Items.Max(i => i.Position) + 1 : 1;
            var created = await _apiClient.CreateItemAsync(SelectedList.Id, text, note, position);
            Items.Add(created);
            NewItemText = string.Empty;
            NewItemNote = string.Empty;
            ReorderItemsByPosition();
        });
    }

    [RelayCommand]
    private async Task EditItemAsync(ListItemModel? item)
    {
        if (SelectedList is null || item is null)
        {
            return;
        }

        var editedText = await _dialogService.PromptAsync("Edit Item", "Item text", item.Text, 600);
        if (editedText is null)
        {
            return;
        }

        var editedNote = await _dialogService.PromptAsync("Edit Note", "Note", item.Note, 2000);
        if (editedNote is null)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            var updated = await _apiClient.UpdateItemAsync(SelectedList.Id, item.Id, editedText.Trim(), editedNote.Trim());
            ReplaceItem(updated);
            ReorderItemsByPosition();
        });
    }

    [RelayCommand]
    private async Task DeleteItemAsync(ListItemModel? item)
    {
        if (SelectedList is null || item is null)
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmAsync("Delete Item", "Delete this item?", "Delete", "Cancel");
        if (!confirmed)
        {
            return;
        }

        await RunBusyAsync(async () =>
        {
            await _apiClient.DeleteItemAsync(SelectedList.Id, item.Id);
            Items.Remove(item);
        });
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await RunBusyAsync(async () =>
        {
            await _authService.SignOutAsync();
            await _navigationService.NavigateToLoginAsync();
        });
    }

    private async Task LoadListsCoreAsync()
    {
        var lists = await _apiClient.GetListsAsync();
        _suppressSelectionLoad = true;
        try
        {
            ReplaceCollection(Lists, lists.OrderByDescending(l => l.UpdatedAt));
            SelectedList = Lists.FirstOrDefault();
        }
        finally
        {
            _suppressSelectionLoad = false;
        }

        await LoadSelectedListItemsCoreAsync();
    }

    private async Task LoadSelectedListItemsSafeAsync()
    {
        try
        {
            await RunBusyAsync(LoadSelectedListItemsCoreAsync);
        }
        catch
        {
            // RunBusyAsync captures and exposes errors.
        }
    }

    private async Task LoadSelectedListItemsCoreAsync()
    {
        if (SelectedList is null)
        {
            Items.Clear();
            return;
        }

        var items = await _apiClient.GetItemsAsync(SelectedList.Id);
        ReplaceCollection(Items, items.OrderBy(i => i.Position));
    }

    private void RenderSummary(ListModel? list)
    {
        SummaryTitle = list?.Title ?? "No list selected";
        SummaryText = list is null
            ? "Create a list to get started."
            : string.IsNullOrWhiteSpace(list.Summary)
                ? "No summary yet."
                : list.Summary;
    }

    private void ReplaceList(ListModel updated)
    {
        var existing = Lists.FirstOrDefault(l => l.Id == updated.Id);
        if (existing is null)
        {
            Lists.Add(updated);
            return;
        }

        var index = Lists.IndexOf(existing);
        Lists[index] = updated;
    }

    private void ReorderListsByUpdatedAt()
    {
        ReplaceCollection(Lists, Lists.OrderByDescending(l => l.UpdatedAt).ToList());
    }

    private void ReplaceItem(ListItemModel updated)
    {
        var existing = Items.FirstOrDefault(i => i.Id == updated.Id);
        if (existing is null)
        {
            Items.Add(updated);
            return;
        }

        var index = Items.IndexOf(existing);
        Items[index] = updated;
    }

    private void ReorderItemsByPosition()
    {
        ReplaceCollection(Items, Items.OrderBy(i => i.Position).ToList());
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> collection, IEnumerable<T> values)
    {
        collection.Clear();
        foreach (var value in values)
        {
            collection.Add(value);
        }
    }
}




