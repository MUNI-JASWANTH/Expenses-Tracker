using FirstMauiApp.Models;
using FirstMauiApp.ViewModels;

namespace FirstMauiApp.Views;

public partial class AddTransactionPage : ContentPage, IQueryAttributable
{
    private readonly AddTransactionViewModel _viewModel;
    private Transaction? _transaction;
    private bool _initialized;

    public AddTransactionPage(AddTransactionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Transaction", out var value) && value is Transaction transaction)
            _transaction = transaction;
        _initialized = false;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_initialized) return;
        _initialized = true;
        await _viewModel.InitializeAsync(_transaction);
    }
}
