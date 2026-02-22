using CommunityToolkit.Mvvm.ComponentModel;

namespace SummaryLists.Mobile.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotBusy));
    }

    protected async Task RunBusyAsync(Func<Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            await action();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
