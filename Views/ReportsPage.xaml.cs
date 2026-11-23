using FirstMauiApp.ViewModels;

namespace FirstMauiApp.Views;

public partial class ReportsPage : ContentPage
{
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as ReportsViewModel)?.LoadDataAsync();
    }
}
