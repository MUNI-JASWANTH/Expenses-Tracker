using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstMauiApp.Models;
using FirstMauiApp.Services;

namespace FirstMauiApp.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    public partial FinancialSummary? Summary { get; set; }

    [ObservableProperty]
    public partial int SelectedYear { get; set; } = DateTime.Now.Year;

    [ObservableProperty]
    public partial int SelectedMonth { get; set; } = DateTime.Now.Month;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    [ObservableProperty]
    public partial ObservableCollection<CategoryData> ExpenseCategories { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<CategoryData> IncomeCategories { get; set; } = new();

    [ObservableProperty]
    public partial int TransactionCount { get; set; }

    [ObservableProperty]
    public partial decimal AverageExpense { get; set; }

    [ObservableProperty]
    public partial string TopExpenseCategory { get; set; } = "—";

    [ObservableProperty]
    public partial string TopIncomeSource { get; set; } = "—";

    public ReportsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Reports";
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            Summary = await _databaseService.GetMonthlySummaryAsync(SelectedYear, SelectedMonth);

            var transactions = await _databaseService.GetTransactionsByMonthAsync(SelectedYear, SelectedMonth);
            TransactionCount = transactions.Count;

            var expenseCount = transactions.Count(t => t.Type == TransactionType.Expense);
            AverageExpense = expenseCount > 0 ? Summary.TotalExpense / expenseCount : 0;

            ExpenseCategories.Clear();
            foreach (var kvp in Summary.ExpensesByCategory.OrderByDescending(x => x.Value))
            {
                ExpenseCategories.Add(new CategoryData
                {
                    Category = kvp.Key,
                    Amount = kvp.Value,
                    Percentage = Summary.TotalExpense > 0 ? (double)(kvp.Value / Summary.TotalExpense * 100) : 0
                });
            }

            TopExpenseCategory = ExpenseCategories.Count > 0 ? ExpenseCategories[0].CategoryDisplay : "—";

            IncomeCategories.Clear();
            foreach (var kvp in Summary.IncomeByCategory.OrderByDescending(x => x.Value))
            {
                IncomeCategories.Add(new CategoryData
                {
                    Category = kvp.Key,
                    Amount = kvp.Value,
                    Percentage = Summary.TotalIncome > 0 ? (double)(kvp.Value / Summary.TotalIncome * 100) : 0
                });
            }

            TopIncomeSource = IncomeCategories.Count > 0 ? IncomeCategories[0].CategoryDisplay : "—";
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
    async Task ExportData()
    {
        if (Summary == null) return;

        try
        {
            var csv = new StringBuilder();
            csv.AppendLine("Date,Type,Category,Amount,Description");

            var transactions = await _databaseService.GetTransactionsByMonthAsync(SelectedYear, SelectedMonth);
            foreach (var t in transactions)
            {
                csv.Append(t.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',')
                    .Append(t.Type).Append(',').Append(t.Category).Append(',')
                    .Append(t.Amount.ToString("F2", CultureInfo.InvariantCulture)).Append(',')
                    .AppendLine(EscapeCsv(t.Description));
            }

            var fileName = $"finance_report_{SelectedYear}_{SelectedMonth:D2}.csv";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, csv.ToString());

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = $"Finance Report - {MonthYearDisplay}",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await ReportErrorAsync(ex);
        }
    }

    // Escape CSV formula prefixes so spreadsheet programs treat descriptions as data.
    private static string EscapeCsv(string value)
    {
        var safe = value ?? string.Empty;
        if (safe.Length > 0 && (safe[0] is '=' or '+' or '-' or '@' or '\t' or '\r' or '\n'))
            safe = "'" + safe;
        return "\"" + safe.Replace("\"", "\"\"") + "\"";
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

public class CategoryData
{
    public TransactionCategory Category { get; set; }
    public decimal Amount { get; set; }
    public double Percentage { get; set; }

    public double ProgressValue => Percentage > 0 ? Percentage / 100.0 : 0.0;
    public string CategoryDisplay => Category.ToString();
    public string FormattedAmount => Amount.ToString("C");
    public string FormattedPercentage => $"{Percentage:F1}%";
    public Color CategoryColor => GetCategoryColor();

    private Color GetCategoryColor()
    {
        return Category switch
        {
            TransactionCategory.Food => Color.FromArgb("#FF6B6B"),
            TransactionCategory.Transport => Color.FromArgb("#4ECDC4"),
            TransactionCategory.Housing => Color.FromArgb("#45B7D1"),
            TransactionCategory.Utilities => Color.FromArgb("#FFA07A"),
            TransactionCategory.Entertainment => Color.FromArgb("#98D8C8"),
            TransactionCategory.Healthcare => Color.FromArgb("#F7DC6F"),
            TransactionCategory.Shopping => Color.FromArgb("#BB8FCE"),
            TransactionCategory.Education => Color.FromArgb("#85C1E9"),
            TransactionCategory.Salary => Color.FromArgb("#82E0AA"),
            TransactionCategory.Freelance => Color.FromArgb("#F8B500"),
            TransactionCategory.Investment => Color.FromArgb("#00CED1"),
            TransactionCategory.Gift => Color.FromArgb("#FF69B4"),
            _ => Color.FromArgb("#95A5A6")
        };
    }
}
