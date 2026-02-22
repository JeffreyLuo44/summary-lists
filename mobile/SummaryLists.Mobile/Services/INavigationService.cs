namespace SummaryLists.Mobile.Services;

public interface INavigationService
{
    Task NavigateToLoginAsync();

    Task NavigateToListsAsync();
}
