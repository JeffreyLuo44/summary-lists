namespace SummaryLists.Mobile.Services;

public sealed class DialogService : IDialogService
{
    public async Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        var page = GetCurrentPage();
        if (page is null)
        {
            return false;
        }

        return await page.DisplayAlert(title, message, accept, cancel);
    }

    public async Task<string?> PromptAsync(string title, string message, string initialValue, int maxLength = -1)
    {
        var page = GetCurrentPage();
        if (page is null)
        {
            return null;
        }

        return await page.DisplayPromptAsync(
            title,
            message,
            initialValue: initialValue,
            maxLength: maxLength > 0 ? maxLength : -1);
    }

    private static Page? GetCurrentPage()
    {
        var rootPage = Application.Current?.Windows.FirstOrDefault()?.Page;
        return rootPage switch
        {
            NavigationPage navPage => navPage.CurrentPage,
            _ => rootPage,
        };
    }
}
