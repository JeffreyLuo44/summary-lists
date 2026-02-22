using SummaryLists.Mobile.Models;
using SummaryLists.Mobile.Services;

namespace SummaryLists.Mobile.Tests.TestDoubles;

internal sealed class FakeAuthService : IAuthService
{
    public AuthSession? Session { get; set; }

    public int SignInCalls { get; private set; }

    public int RegisterCalls { get; private set; }

    public int SignOutCalls { get; private set; }

    public string LastEmail { get; private set; } = string.Empty;

    public string LastPassword { get; private set; } = string.Empty;

    public Task<AuthSession?> GetSessionAsync()
    {
        return Task.FromResult(Session);
    }

    public Task<AuthSession> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        RegisterCalls++;
        LastEmail = email;
        LastPassword = password;
        Session = new AuthSession
        {
            IdToken = "token-register",
            RefreshToken = "refresh-register",
            Email = email,
        };
        return Task.FromResult(Session);
    }

    public Task<AuthSession> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        SignInCalls++;
        LastEmail = email;
        LastPassword = password;
        Session = new AuthSession
        {
            IdToken = "token-login",
            RefreshToken = "refresh-login",
            Email = email,
        };
        return Task.FromResult(Session);
    }

    public Task SignOutAsync()
    {
        SignOutCalls++;
        Session = null;
        return Task.CompletedTask;
    }
}

internal sealed class FakeApiClient : IApiClient
{
    private readonly List<ListModel> _lists = [];
    private readonly Dictionary<string, List<ListItemModel>> _items = new();
    private int _listCounter = 1;
    private int _itemCounter = 1;

    public void SetLists(params ListModel[] lists)
    {
        _lists.Clear();
        _lists.AddRange(lists);
    }

    public void SetItems(string listId, params ListItemModel[] items)
    {
        _items[listId] = items.ToList();
    }

    public Task<IReadOnlyList<ListModel>> GetListsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ListModel>>(_lists.Select(CloneList).ToList());
    }

    public Task<ListModel> CreateListAsync(string title, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow.ToString("O");
        var created = new ListModel
        {
            Id = $"list-{_listCounter++}",
            UserId = "user-1",
            Title = title,
            Summary = $"This list tracks: {title}.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        _lists.Add(created);
        _items[created.Id] = [];
        return Task.FromResult(CloneList(created));
    }

    public Task<ListModel> UpdateListAsync(string listId, string title, CancellationToken cancellationToken = default)
    {
        var existing = _lists.First(l => l.Id == listId);
        existing.Title = title;
        existing.UpdatedAt = DateTimeOffset.UtcNow.ToString("O");
        return Task.FromResult(CloneList(existing));
    }

    public Task DeleteListAsync(string listId, CancellationToken cancellationToken = default)
    {
        _lists.RemoveAll(l => l.Id == listId);
        _items.Remove(listId);
        return Task.CompletedTask;
    }

    public Task<ListModel> RegenerateSummaryAsync(string listId, CancellationToken cancellationToken = default)
    {
        var existing = _lists.First(l => l.Id == listId);
        existing.Summary = $"Regenerated summary for {existing.Title}.";
        existing.UpdatedAt = DateTimeOffset.UtcNow.ToString("O");
        return Task.FromResult(CloneList(existing));
    }

    public Task<IReadOnlyList<ListItemModel>> GetItemsAsync(string listId, CancellationToken cancellationToken = default)
    {
        if (!_items.TryGetValue(listId, out var values))
        {
            values = [];
            _items[listId] = values;
        }
        return Task.FromResult<IReadOnlyList<ListItemModel>>(values.Select(CloneItem).ToList());
    }

    public Task<ListItemModel> CreateItemAsync(
        string listId,
        string text,
        string note,
        double position,
        CancellationToken cancellationToken = default)
    {
        if (!_items.TryGetValue(listId, out var values))
        {
            values = [];
            _items[listId] = values;
        }

        var now = DateTimeOffset.UtcNow.ToString("O");
        var created = new ListItemModel
        {
            Id = $"item-{_itemCounter++}",
            UserId = "user-1",
            ListId = listId,
            Text = text,
            Note = note,
            Position = position,
            CreatedAt = now,
            UpdatedAt = now,
        };
        values.Add(created);
        return Task.FromResult(CloneItem(created));
    }

    public Task<ListItemModel> UpdateItemAsync(
        string listId,
        string itemId,
        string text,
        string note,
        CancellationToken cancellationToken = default)
    {
        var existing = _items[listId].First(i => i.Id == itemId);
        existing.Text = text;
        existing.Note = note;
        existing.UpdatedAt = DateTimeOffset.UtcNow.ToString("O");
        return Task.FromResult(CloneItem(existing));
    }

    public Task DeleteItemAsync(string listId, string itemId, CancellationToken cancellationToken = default)
    {
        if (_items.TryGetValue(listId, out var values))
        {
            values.RemoveAll(v => v.Id == itemId);
        }
        return Task.CompletedTask;
    }

    private static ListModel CloneList(ListModel value)
    {
        return new ListModel
        {
            Id = value.Id,
            UserId = value.UserId,
            Title = value.Title,
            Summary = value.Summary,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
        };
    }

    private static ListItemModel CloneItem(ListItemModel value)
    {
        return new ListItemModel
        {
            Id = value.Id,
            UserId = value.UserId,
            ListId = value.ListId,
            Text = value.Text,
            Note = value.Note,
            Position = value.Position,
            CreatedAt = value.CreatedAt,
            UpdatedAt = value.UpdatedAt,
        };
    }
}

internal sealed class FakeNavigationService : INavigationService
{
    public int NavigateToLoginCalls { get; private set; }

    public int NavigateToListsCalls { get; private set; }

    public Task NavigateToLoginAsync()
    {
        NavigateToLoginCalls++;
        return Task.CompletedTask;
    }

    public Task NavigateToListsAsync()
    {
        NavigateToListsCalls++;
        return Task.CompletedTask;
    }
}

internal sealed class FakeDialogService : IDialogService
{
    public bool ConfirmResult { get; set; } = true;

    public Queue<string?> PromptResults { get; } = new();

    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        return Task.FromResult(ConfirmResult);
    }

    public Task<string?> PromptAsync(string title, string message, string initialValue, int maxLength = -1)
    {
        if (PromptResults.Count == 0)
        {
            return Task.FromResult<string?>(initialValue);
        }

        return Task.FromResult(PromptResults.Dequeue());
    }
}
