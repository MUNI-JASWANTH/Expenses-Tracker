using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace FirstMauiApp.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    public bool IsNotBusy => !IsBusy;

    protected async Task ReportErrorAsync(Exception exception)
    {
        if (Shell.Current is not null)
            await Shell.Current.DisplayAlertAsync("Error", exception.Message, "OK");
    }

    protected async Task ExecuteAsync(Func<Task> action)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            await ReportErrorAsync(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
