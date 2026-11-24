using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstMauiApp.Models;
using FirstMauiApp.Services;

namespace FirstMauiApp.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    public partial FinancialSummary? Summary { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Transaction> RecentTransactions { get; set; } = new();

    [ObservableProperty]
    public partial int SelectedYear { get; set; } = DateTime.Now.Year;

    [ObservableProperty]
    public partial int SelectedMonth { get; set; } = DateTime.Now.Month;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial ObservableCollection<BudgetAlert> BudgetAlerts { get; set; } = new();

    [ObservableProperty]
    public partial bool HasBudgetAlerts { get; set; }

    public DashboardViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Dashboard";
    }

    [RelayCommand]
    public async Task LoadData()
    {
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            Summary = await _databaseService.GetMonthlySummaryAsync(SelectedYear, SelectedMonth);

            var transactions = await _databaseService.GetTransactionsByMonthAsync(SelectedYear, SelectedMonth);
            RecentTransactions.Clear();
            foreach (var t in transactions.Take(10))
            {
                RecentTransactions.Add(t);
            }

            // Load budget alerts
            await LoadBudgetAlertsAsync();
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

    private async Task LoadBudgetAlertsAsync()
    {
        BudgetAlerts.Clear();
        var budgets = await _databaseService.GetBudgetsAsync(SelectedYear, SelectedMonth);
        var expensesByCategory = Summary?.ExpensesByCategory ?? new();

        foreach (var budget in budgets)
        {
            var spent = expensesByCategory.GetValueOrDefault(budget.Category, 0);
            var percentage = budget.MonthlyLimit > 0 ? (double)(spent / budget.MonthlyLimit * 100) : 0;

            if (percentage >= 80) // Alert at 80% or more
            {
                BudgetAlerts.Add(new BudgetAlert
                {
                    Category = budget.Category,
                    Spent = spent,
                    Limit = budget.MonthlyLimit,
                    Percentage = percentage,
                    IsOverBudget = spent > budget.MonthlyLimit
                });
            }
        }

        HasBudgetAlerts = BudgetAlerts.Count > 0;
    }

    [RelayCommand]
    async Task PreviousMonth()
    {
        if (SelectedMonth == 1)
        {
            SelectedMonth = 12;
            SelectedYear--;
        }
        else
        {
            SelectedMonth--;
        }
        await LoadDataAsync();
    }

    [RelayCommand]
    async Task NextMonth()
    {
        if (SelectedMonth == 12)
        {
            SelectedMonth = 1;
            SelectedYear++;
        }
        else
        {
            SelectedMonth++;
        }
        await LoadDataAsync();
    }

    [RelayCommand]
    async Task CurrentMonth()
    {
        SelectedYear = DateTime.Now.Year;
        SelectedMonth = DateTime.Now.Month;
        await LoadDataAsync();
    }

    [RelayCommand]
    async Task AddTransaction()
    {
        await Shell.Current.GoToAsync(nameof(Views.AddTransactionPage));
    }

    [RelayCommand]
    async Task ViewAllTransactions()
    {
        await Shell.Current.GoToAsync("//TransactionsPage");
    }

    [RelayCommand]
    async Task ViewReports()
    {
        await Shell.Current.GoToAsync("//ReportsPage");
    }

    [RelayCommand]
    async Task ViewBudgets()
    {
        await Shell.Current.GoToAsync("//BudgetsPage");
    }

    public string MonthYearDisplay => new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy");

    partial void OnSelectedMonthChanged(int value)
    {
        OnPropertyChanged(nameof(MonthYearDisplay));
    }

    partial void OnSelectedYearChanged(int value)
    {
        OnPropertyChanged(nameof(MonthYearDisplay));
    }
}

public class BudgetAlert
{
    public TransactionCategory Category { get; set; }
    public decimal Spent { get; set; }
    public decimal Limit { get; set; }
    public double Percentage { get; set; }
    public bool IsOverBudget { get; set; }

    public string CategoryDisplay => Category.ToString();
    public string FormattedSpent => CurrencySettings.Format(Spent);
    public string FormattedLimit => CurrencySettings.Format(Limit);
    public string FormattedPercentage => $"{Percentage:F0}%";
    public string AlertMessage => IsOverBudget
        ? $"⚠️ {CategoryDisplay}: Over budget! {FormattedSpent} / {FormattedLimit}"
        : $"⚡ {CategoryDisplay}: {FormattedPercentage} used ({FormattedSpent} / {FormattedLimit})";
    public Color AlertColor => IsOverBudget ? Colors.Red : Colors.Orange;
}
