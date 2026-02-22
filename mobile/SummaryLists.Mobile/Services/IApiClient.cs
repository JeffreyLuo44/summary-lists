using SummaryLists.Mobile.Models;

namespace SummaryLists.Mobile.Services;

public interface IApiClient
{
    Task<IReadOnlyList<ListModel>> GetListsAsync(CancellationToken cancellationToken = default);

    Task<ListModel> CreateListAsync(string title, CancellationToken cancellationToken = default);

    Task<ListModel> UpdateListAsync(string listId, string title, CancellationToken cancellationToken = default);

    Task DeleteListAsync(string listId, CancellationToken cancellationToken = default);

    Task<ListModel> RegenerateSummaryAsync(string listId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ListItemModel>> GetItemsAsync(string listId, CancellationToken cancellationToken = default);

    Task<ListItemModel> CreateItemAsync(
        string listId,
        string text,
        string note,
        double position,
        CancellationToken cancellationToken = default);

    Task<ListItemModel> UpdateItemAsync(
        string listId,
        string itemId,
        string text,
        string note,
        CancellationToken cancellationToken = default);

    Task DeleteItemAsync(string listId, string itemId, CancellationToken cancellationToken = default);
}
