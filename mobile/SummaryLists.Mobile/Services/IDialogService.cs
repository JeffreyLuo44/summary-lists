namespace SummaryLists.Mobile.Services;

public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);

    Task<string?> PromptAsync(string title, string message, string initialValue, int maxLength = -1);
}
