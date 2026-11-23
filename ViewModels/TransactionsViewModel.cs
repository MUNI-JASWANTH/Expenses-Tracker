using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstMauiApp.Models;
using FirstMauiApp.Services;

namespace FirstMauiApp.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    public partial ObservableCollection<Transaction> Transactions { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<Transaction> FilteredTransactions { get; set; } = new();

    // Live search across description, category, and transaction type.
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
        CalculateTotals();
    }

    [ObservableProperty]
    public partial TransactionType? FilterType { get; set; }

    [ObservableProperty]
    public partial TransactionCategory? FilterCategory { get; set; }

    [ObservableProperty]
    public partial DateTime StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

    [ObservableProperty]
    public partial DateTime EndDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial decimal TotalIncome { get; set; }

    [ObservableProperty]
    public partial decimal TotalExpense { get; set; }

    public decimal NetBalance => TotalIncome - TotalExpense;
    public string FormattedNetBalance => NetBalance.ToString("C");

    public TransactionsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Transactions";
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            if (StartDate.Date > EndDate.Date)
            {
                FilteredTransactions.Clear();
                Transactions.Clear();
                TotalIncome = 0;
                TotalExpense = 0;
                OnPropertyChanged(nameof(NetBalance));
                OnPropertyChanged(nameof(FormattedNetBalance));
                return;
            }

            var allTransactions = await _databaseService.GetTransactionsByDateRangeAsync(StartDate, EndDate);
            Transactions.Clear();
            foreach (var t in allTransactions)
            {
                Transactions.Add(t);
            }

            ApplyFilters();
            CalculateTotals();
        }
        catch (Exception ex)
        {
            await ReportErrorAsync(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    async Task ApplyFiltersAsync()
    {
        await LoadDataAsync();
    }

    [RelayCommand]
    void ClearFilters()
    {
        FilterType = null;
        FilterCategory = null;
        SearchText = string.Empty;
        StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        EndDate = DateTime.Now;
        _ = LoadDataAsync();
    }

    void ApplyFilters()
    {
        var filtered = Transactions.AsEnumerable();

        if (FilterType.HasValue)
        {
            filtered = filtered.Where(t => t.Type == FilterType.Value);
        }

        if (FilterCategory.HasValue)
        {
            filtered = filtered.Where(t => t.Category == FilterCategory.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var query = SearchText.Trim();
            filtered = filtered.Where(t =>
                t.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Category.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                t.Type.ToString().Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        FilteredTransactions.Clear();
        foreach (var t in filtered.OrderByDescending(t => t.Date))
        {
            FilteredTransactions.Add(t);
        }
    }

    void CalculateTotals()
    {
        TotalIncome = FilteredTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        TotalExpense = FilteredTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        OnPropertyChanged(nameof(NetBalance));
        OnPropertyChanged(nameof(FormattedNetBalance));
    }

    [RelayCommand]
    async Task AddTransaction()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddTransactionPage));
    }

    [RelayCommand]
    async Task EditTransaction(Transaction transaction)
    {
        if (transaction == null) return;
        var parameters = new Dictionary<string, object>
        {
            { "Transaction", transaction }
        };
        await Shell.Current.GoToAsync(nameof(Views.AddTransactionPage), parameters);
    }

    [RelayCommand]
    async Task DeleteTransaction(Transaction transaction)
    {
        if (transaction == null) return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Delete Transaction",
            $"Are you sure you want to delete '{transaction.Description}'?",
            "Yes",
            "No");

        if (confirmed)
        {
            try
            {
                await _databaseService.DeleteTransactionAsync(transaction);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                await ReportErrorAsync(ex);
            }
        }
    }

    [RelayCommand]
    async Task Refresh()
    {
        await LoadDataAsync();
    }
}
