using FirstMauiApp.ViewModels;

namespace FirstMauiApp.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TransactionsViewModel vm)
            await vm.LoadDataAsync();
    }
}
