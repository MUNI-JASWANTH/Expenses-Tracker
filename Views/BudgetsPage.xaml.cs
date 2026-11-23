using FirstMauiApp.ViewModels;

namespace FirstMauiApp.Views;

public partial class BudgetsPage : ContentPage
{
    public BudgetsPage(BudgetsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await (BindingContext as BudgetsViewModel)?.LoadDataAsync();
    }
}
