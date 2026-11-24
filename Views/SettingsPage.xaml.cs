using FirstMauiApp.Services;

namespace FirstMauiApp.Views;

public partial class SettingsPage : ContentPage
{
    private bool _initializing;

    public SettingsPage()
    {
        InitializeComponent();
        _initializing = true;
        foreach (var code in CurrencySettings.Codes)
            CurrencyPicker.Items.Add(code);
        CurrencyPicker.SelectedItem = CurrencySettings.SelectedCode;
        _initializing = false;
        UpdateExample();
    }

    private void OnCurrencyChanged(object? sender, EventArgs e)
    {
        if (_initializing || CurrencyPicker.SelectedItem is not string code)
            return;
        CurrencySettings.SelectedCode = code;
        UpdateExample();
    }

    private void UpdateExample() =>
        ExampleLabel.Text = $"Example: {CurrencySettings.Format(12345.67m)}";
}
