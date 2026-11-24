using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstMauiApp.Models;
using FirstMauiApp.Services;

namespace FirstMauiApp.ViewModels;

public partial class BudgetsViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    public partial ObservableCollection<BudgetItem> BudgetItems { get; set; } = new();

    [ObservableProperty]
    public partial int SelectedYear { get; set; } = DateTime.Now.Year;

    [ObservableProperty]
    public partial int SelectedMonth { get; set; } = DateTime.Now.Month;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial decimal TotalBudget { get; set; }

    [ObservableProperty]
    public partial decimal TotalSpent { get; set; }

    [ObservableProperty]
    public partial decimal TotalRemaining { get; set; }

    [ObservableProperty]
    public partial int BudgetsSetCount { get; set; }

    public BudgetsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Budgets";
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var summary = await _databaseService.GetMonthlySummaryAsync(SelectedYear, SelectedMonth);
            var budgets = await _databaseService.GetBudgetsAsync(SelectedYear, SelectedMonth);

            BudgetItems.Clear();

            var expenseCategories = Enum.GetValues<TransactionCategory>()
                .Where(c => c >= TransactionCategory.Food && c <= TransactionCategory.OtherExpense)
                .ToList();

            decimal totalBudget = 0;
            decimal totalSpent = 0;
            int budgetsSet = 0;

            foreach (var category in expenseCategories)
            {
                var spent = summary.ExpensesByCategory.GetValueOrDefault(category, 0);
                var budget = budgets.FirstOrDefault(b => b.Category == category);
                var limit = budget?.MonthlyLimit ?? 0;

                BudgetItems.Add(new BudgetItem
                {
                    Category = category,
                    Spent = spent,
                    Limit = limit,
                    BudgetId = budget?.Id ?? 0
                });

                if (limit > 0)
                {
                    totalBudget += limit;
                    budgetsSet++;
                }
                totalSpent += spent;
            }

            TotalBudget = totalBudget;
            TotalSpent = totalSpent;
            TotalRemaining = totalBudget - totalSpent;
            BudgetsSetCount = budgetsSet;
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
    async Task SetBudget(BudgetItem item)
    {
        if (item is null) return;

        var limitString = await Shell.Current.DisplayPromptAsync(
            $"Set Budget for {item.CategoryDisplay}",
            "Enter monthly limit:",
            initialValue: item.Limit > 0 ? item.Limit.ToString("F2") : "",
            keyboard: Keyboard.Numeric);

        if (limitString is null) return; // User cancelled

        if (decimal.TryParse(limitString, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.CurrentCulture, out decimal limit) && limit >= 0)
        {
            var budget = await _databaseService.GetBudgetAsync(item.Category, SelectedYear, SelectedMonth);

            if (limit == 0)
            {
                if (budget is not null)
                    await _databaseService.DeleteBudgetAsync(budget);
            }
            else
            {
                budget ??= new Budget
                {
                    Category = item.Category,
                    Month = SelectedMonth,
                    Year = SelectedYear
                };
                budget.MonthlyLimit = limit;
                await _databaseService.SaveBudgetAsync(budget);
            }

            await LoadDataAsync();
        }
        else
        {
            await Shell.Current.DisplayAlertAsync("Invalid Input", "Please enter a valid number.", "OK");
        }
    }

    [RelayCommand]
    async Task RemoveBudget(BudgetItem item)
    {
        if (item.BudgetId == 0) return;

        var confirmed = await Shell.Current.DisplayAlertAsync(
            "Remove Budget",
            $"Remove budget for {item.CategoryDisplay}?",
            "Yes",
            "No");

        if (confirmed)
        {
            var budget = await _databaseService.GetBudgetAsync(item.Category, SelectedYear, SelectedMonth);
            if (budget != null)
            {
                await _databaseService.DeleteBudgetAsync(budget);
                await LoadDataAsync();
            }
        }
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

public class BudgetItem
{
    public TransactionCategory Category { get; set; }
    public decimal Spent { get; set; }
    public decimal Limit { get; set; }
    public int BudgetId { get; set; }

    public string CategoryDisplay => Category.ToString();
    public string FormattedSpent => CurrencySettings.Format(Spent);
    public string FormattedLimit => Limit > 0 ? CurrencySettings.Format(Limit) : "Not set";
    public string FormattedRemaining => Limit > 0 ? CurrencySettings.Format(Limit - Spent) : "—";
    public double Progress => Limit > 0 ? Math.Clamp((double)(Spent / Limit), 0d, 1d) : 0;
    public bool HasBudget => Limit > 0;
    public bool IsOverBudget => Limit > 0 && Spent > Limit;
    public Color ProgressColor => GetProgressColor();
    public string StatusText => GetStatusText();

    private Color GetProgressColor()
    {
        if (Limit == 0) return Colors.Gray;
        if (Spent > Limit) return Colors.Red;
        if (Spent > Limit * 0.8m) return Colors.Orange;
        if (Spent > Limit * 0.5m) return Colors.Yellow;
        return Colors.Green;
    }

    private string GetStatusText()
    {
        if (!HasBudget) return "No budget set";
        if (IsOverBudget) return $"Over by {CurrencySettings.Format(Spent - Limit)}";
        var pct = (double)(Spent / Limit * 100);
        return $"{pct:F0}% used";
    }
}
