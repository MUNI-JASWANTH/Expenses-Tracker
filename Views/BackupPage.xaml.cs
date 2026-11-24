using FirstMauiApp.Services;

namespace FirstMauiApp.Views;

public partial class BackupPage : ContentPage
{
    private readonly BackupService _backups;
    private bool _busy;

    public BackupPage(BackupService backups)
    {
        InitializeComponent();
        _backups = backups;
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            var path = await _backups.CreateBackupAsync();
            StatusLabel.Text = "Backup created. Choose a safe destination in the share dialog.";
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Save expense tracker backup",
                File = new ShareFile(path, "application/json")
            });
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Export failed: " + ex.Message;
            await DisplayAlert("Export failed", ex.Message, "OK");
        }
        finally { SetBusy(false); }
    }

    private async void OnRestoreClicked(object sender, EventArgs e)
    {
        if (_busy) return;
        SetBusy(true);
        try
        {
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose an expense tracker JSON backup"
            });
            if (file is null) return;

            bool confirmed = await DisplayAlert("Replace existing data?",
                "All transactions and budgets on this device will be replaced by the selected backup. This cannot be undone. Continue?",
                "Replace data", "Cancel");
            if (!confirmed) return;

            await using var stream = await file.OpenReadAsync();
            var counts = await _backups.RestoreBackupAsync(stream);
            StatusLabel.Text = $"Restored {counts.Transactions} transactions and {counts.Budgets} budgets. Reopen the other tabs to refresh.";
            await DisplayAlert("Restore complete", StatusLabel.Text, "OK");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Restore failed. Existing records should remain unchanged. " + ex.Message;
            await DisplayAlert("Restore failed", ex.Message, "OK");
        }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        ExportButton.IsEnabled = !busy;
        RestoreButton.IsEnabled = !busy;
    }
}
