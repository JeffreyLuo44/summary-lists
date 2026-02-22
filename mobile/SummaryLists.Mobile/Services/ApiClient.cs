using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SummaryLists.Mobile.Config;
using SummaryLists.Mobile.Models;

namespace SummaryLists.Mobile.Services;

public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public ApiClient(HttpClient httpClient, IAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<IReadOnlyList<ListModel>> GetListsAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ListsResponse>(HttpMethod.Get, "/lists", null, cancellationToken);
        return response?.Lists ?? [];
    }

    public async Task<ListModel> CreateListAsync(string title, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ListResponse>(
            HttpMethod.Post,
            "/lists",
            new { title },
            cancellationToken);
        return response?.List ?? throw new InvalidOperationException("Missing list response.");
    }

    public async Task<ListModel> UpdateListAsync(string listId, string title, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ListResponse>(
            HttpMethod.Patch,
            $"/lists/{listId}",
            new { title },
            cancellationToken);
        return response?.List ?? throw new InvalidOperationException("Missing list response.");
    }

    public Task DeleteListAsync(string listId, CancellationToken cancellationToken = default)
    {
        return SendAsync<object>(HttpMethod.Delete, $"/lists/{listId}", null, cancellationToken);
    }

    public async Task<ListModel> RegenerateSummaryAsync(string listId, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ListResponse>(
            HttpMethod.Post,
            $"/lists/{listId}/regenerate-summary",
            new { },
            cancellationToken);
        return response?.List ?? throw new InvalidOperationException("Missing list response.");
    }

    public async Task<IReadOnlyList<ListItemModel>> GetItemsAsync(string listId, CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ItemsResponse>(HttpMethod.Get, $"/lists/{listId}/items", null, cancellationToken);
        return response?.Items ?? [];
    }

    public async Task<ListItemModel> CreateItemAsync(
        string listId,
        string text,
        string note,
        double position,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ItemResponse>(
            HttpMethod.Post,
            $"/lists/{listId}/items",
            new { text, note, position },
            cancellationToken);
        return response?.Item ?? throw new InvalidOperationException("Missing item response.");
    }

    public async Task<ListItemModel> UpdateItemAsync(
        string listId,
        string itemId,
        string text,
        string note,
        CancellationToken cancellationToken = default)
    {
        var response = await SendAsync<ItemResponse>(
            HttpMethod.Patch,
            $"/lists/{listId}/items/{itemId}",
            new { text, note },
            cancellationToken);
        return response?.Item ?? throw new InvalidOperationException("Missing item response.");
    }

    public Task DeleteItemAsync(string listId, string itemId, CancellationToken cancellationToken = default)
    {
        return SendAsync<object>(HttpMethod.Delete, $"/lists/{listId}/items/{itemId}", null, cancellationToken);
    }

    private async Task<T?> SendAsync<T>(
        HttpMethod method,
        string route,
        object? payload,
        CancellationToken cancellationToken)
    {
        if (AppConfig.ApiBaseUrl.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Set AppConfig.ApiBaseUrl before calling the API.");
        }

        var session = await _authService.GetSessionAsync();
        if (session is null || string.IsNullOrWhiteSpace(session.IdToken))
        {
            throw new InvalidOperationException("Not authenticated.");
        }

        var url = $"{AppConfig.ApiBaseUrl.TrimEnd('/')}{route}";
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.IdToken);
        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ParseApiError(content));
        }

        if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    private static string ParseApiError(string responseBody)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;
            var errorCode = root.TryGetProperty("error", out var error)
                ? (error.GetString() ?? "Request failed.")
                : "Request failed.";

            if (root.TryGetProperty("details", out var details))
            {
                var detailText = details.GetString();
                if (!string.IsNullOrWhiteSpace(detailText))
                {
                    return $"{errorCode}: {detailText}";
                }
            }

            return errorCode;
        }
        catch
        {
            // ignored
        }

        return "Request failed.";
    }

    private sealed class ListsResponse
    {
        public List<ListModel>? Lists { get; set; }
    }

    private sealed class ListResponse
    {
        public ListModel? List { get; set; }
    }

    private sealed class ItemsResponse
    {
        public List<ListItemModel>? Items { get; set; }
    }

    private sealed class ItemResponse
    {
        public ListItemModel? Item { get; set; }
    }
}
