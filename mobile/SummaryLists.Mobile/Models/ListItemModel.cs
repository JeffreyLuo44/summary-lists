namespace SummaryLists.Mobile.Models;

public sealed class ListItemModel
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string ListId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public double Position { get; set; }

    public string CreatedAt { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;
}
