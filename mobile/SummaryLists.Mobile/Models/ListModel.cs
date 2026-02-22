using CommunityToolkit.Mvvm.ComponentModel;

namespace SummaryLists.Mobile.Models;

public partial class ListModel : ObservableObject
{
    public string Id { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string CreatedAt { get; set; } = string.Empty;

    public string UpdatedAt { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isSelected;
}
