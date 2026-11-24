using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstMauiApp.Models;
using FirstMauiApp.Services;

namespace FirstMauiApp.ViewModels;

public partial class AddTransactionViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private Transaction? _editingTransaction;

    [ObservableProperty]
    public partial TransactionType SelectedType { get; set; } = TransactionType.Expense;

    [ObservableProperty]
    public partial TransactionCategory SelectedCategory { get; set; } = TransactionCategory.Food;

    [ObservableProperty]
    public partial string Amount { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Description { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial TimeSpan SelectedTime { get; set; } = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    public partial ObservableCollection<TransactionCategory> IncomeCategories { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<TransactionCategory> ExpenseCategories { get; set; } = new();

    [ObservableProperty]
    public partial bool IsEditing { get; set; }

    public AddTransactionViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        Title = "Add Transaction";

        // Initialize categories
        IncomeCategories.Add(TransactionCategory.Salary);
        IncomeCategories.Add(TransactionCategory.Freelance);
        IncomeCategories.Add(TransactionCategory.Investment);
        IncomeCategories.Add(TransactionCategory.Gift);
        IncomeCategories.Add(TransactionCategory.OtherIncome);

        ExpenseCategories.Add(TransactionCategory.Food);
        ExpenseCategories.Add(TransactionCategory.Transport);
        ExpenseCategories.Add(TransactionCategory.Housing);
        ExpenseCategories.Add(TransactionCategory.Utilities);
        ExpenseCategories.Add(TransactionCategory.Entertainment);
        ExpenseCategories.Add(TransactionCategory.Healthcare);
        ExpenseCategories.Add(TransactionCategory.Shopping);
        ExpenseCategories.Add(TransactionCategory.Education);
        ExpenseCategories.Add(TransactionCategory.OtherExpense);
    }

    public Task InitializeAsync(Transaction? transaction = null)
    {
        _editingTransaction = transaction;

        if (transaction != null)
        {
            IsEditing = true;
            Title = "Edit Transaction";
            SelectedType = transaction.Type;
            SelectedCategory = transaction.Category;
            Amount = transaction.Amount.ToString("F2", CultureInfo.CurrentCulture);
            Description = transaction.Description;
            SelectedDate = transaction.Date;
            SelectedTime = transaction.Date.TimeOfDay;
        }
        else
        {
            IsEditing = false;
            Title = "Add Transaction";
            ResetForm();
        }

        return Task.CompletedTask;
    }

    void ResetForm()
    {
        SelectedType = TransactionType.Expense;
        SelectedCategory = TransactionCategory.Food;
        Amount = string.Empty;
        Description = string.Empty;
        SelectedDate = DateTime.Now;
        SelectedTime = DateTime.Now.TimeOfDay;
    }

    partial void OnSelectedTypeChanged(TransactionType value)
    {
        // Reset category when type changes (only if not editing)
        if (!IsEditing || _editingTransaction?.Type != value)
        {
            if (value == TransactionType.Income)
            {
                SelectedCategory = TransactionCategory.Salary;
            }
            else
            {
                SelectedCategory = TransactionCategory.Food;
            }
        }
    }

    [RelayCommand]
    void SelectIncome()
    {
        SelectedType = TransactionType.Income;
    }

    [RelayCommand]
    void SelectExpense()
    {
        SelectedType = TransactionType.Expense;
    }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    [RelayCommand]
    async Task Save()
    {
        if (IsSaving) return;
        if (string.IsNullOrWhiteSpace(Amount))
        {
            await Shell.Current.DisplayAlertAsync("Validation Error", "Please enter an amount", "OK");
            return;
        }

        if (!decimal.TryParse(Amount, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal parsedAmount) || parsedAmount <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Validation Error", "Please enter a valid positive amount", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            await Shell.Current.DisplayAlertAsync("Validation Error", "Please enter a description", "OK");
            return;
        }

        if (!Enum.IsDefined(typeof(TransactionType), SelectedType) || !Enum.IsDefined(typeof(TransactionCategory), SelectedCategory))
        {
            await Shell.Current.DisplayAlertAsync("Validation Error", "Please select a valid transaction type and category", "OK");
            return;
        }

        var validCategories = SelectedType == TransactionType.Income ? IncomeCategories : ExpenseCategories;
        if (!validCategories.Contains(SelectedCategory))
        {
            await Shell.Current.DisplayAlertAsync("Validation Error", "The selected category does not match the transaction type", "OK");
            return;
        }

        // Edit a detached copy: failed saves must not mutate the existing list item.
        var transaction = _editingTransaction is null ? new Transaction() : new Transaction
        {
            Id = _editingTransaction.Id,
            CreatedAt = _editingTransaction.CreatedAt
        };
        transaction.Type = SelectedType;
        transaction.Category = SelectedCategory;
        transaction.Amount = parsedAmount;
        transaction.Description = Description.Trim();
        transaction.Date = SelectedDate.Date + SelectedTime;

        IsSaving = true;
        try
        {
            await _databaseService.SaveTransactionAsync(transaction);
            await Shell.Current.DisplayAlertAsync("Success", _editingTransaction != null ? "Transaction updated" : "Transaction added", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ReportErrorAsync(ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
