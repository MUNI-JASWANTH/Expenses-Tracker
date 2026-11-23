using SQLite;
using FirstMauiApp.Models;

namespace FirstMauiApp.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly string _databasePath;

    public DatabaseService()
    {
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, "finance_tracker.db3");
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        await _initializationLock.WaitAsync();
        try
        {
            if (_database is null)
            {
                var connection = new SQLiteAsyncConnection(_databasePath);
                await connection.CreateTableAsync<Transaction>();
                await connection.CreateTableAsync<Budget>();
                _database = connection;
            }

            return _database;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    // Backup import is a single SQLite transaction: validation occurs before this call,
    // and a failure rolls back deletes and inserts together.
    public async Task<(List<Transaction> Transactions, List<Budget> Budgets)> ReadBackupDataAsync()
    {
        var db = await GetDatabaseAsync();
        var transactions = await db.Table<Transaction>().ToListAsync();
        var budgets = await db.Table<Budget>().ToListAsync();
        return (transactions, budgets);
    }

    public async Task ReplaceBackupDataAsync(List<Transaction> transactions, List<Budget> budgets)
    {
        var db = await GetDatabaseAsync();
        await db.RunInTransactionAsync(connection =>
        {
            connection.DeleteAll<Transaction>();
            connection.DeleteAll<Budget>();
            foreach (var transaction in transactions)
                connection.Insert(transaction);
            foreach (var budget in budgets)
                connection.Insert(budget);
        });
    }

    // Transaction methods
    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Transaction>().OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
            return new List<Transaction>();

        var db = await GetDatabaseAsync();
        // Treat the end date as inclusive, including transactions later in that day.
        var exclusiveEnd = endDate.Date.AddDays(1);
        return await db.Table<Transaction>()
            .Where(t => t.Date >= startDate.Date && t.Date < exclusiveEnd)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByMonthAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return await GetTransactionsByDateRangeAsync(startDate, endDate);
    }

    public async Task<Transaction?> GetTransactionAsync(int id)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Transaction>().FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> SaveTransactionAsync(Transaction transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (transaction.Date.Year < 1 || transaction.Date.Year > 9998)
            throw new ArgumentOutOfRangeException(nameof(transaction.Date), "Transaction date is outside the supported range.");
        if (transaction.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(transaction.Amount), "Transaction amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(transaction.Description))
            throw new ArgumentException("Transaction description is required.", nameof(transaction));
        if (!Enum.IsDefined(typeof(TransactionType), transaction.Type) || !Enum.IsDefined(typeof(TransactionCategory), transaction.Category))
            throw new ArgumentException("Transaction type or category is invalid.", nameof(transaction));
        var categoryMatchesType = transaction.Type == TransactionType.Income
            ? transaction.Category <= TransactionCategory.OtherIncome
            : transaction.Category >= TransactionCategory.Food;
        if (!categoryMatchesType)
            throw new ArgumentException("Transaction category does not match its type.", nameof(transaction));

        var db = await GetDatabaseAsync();
        transaction.Description = transaction.Description.Trim();
        transaction.UpdatedAt = DateTime.Now;

        if (transaction.Id != 0)
        {
            return await db.UpdateAsync(transaction);
        }
        else
        {
            transaction.CreatedAt = DateTime.Now;
            return await db.InsertAsync(transaction);
        }
    }

    public async Task<int> DeleteTransactionAsync(Transaction transaction)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(transaction);
    }

    public async Task<decimal> GetTotalIncomeAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);
        return transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    }

    public async Task<decimal> GetTotalExpenseAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);
        return transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    }

    public async Task<Dictionary<TransactionCategory, decimal>> GetExpensesByCategoryAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);

        return transactions.Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    public async Task<Dictionary<TransactionCategory, decimal>> GetIncomeByCategoryAsync(int year, int month)
    {
        var transactions = await GetTransactionsByMonthAsync(year, month);

        return transactions.Where(t => t.Type == TransactionType.Income)
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
    }

    // Budget methods
    public async Task<List<Budget>> GetBudgetsAsync(int year, int month)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Budget>()
            .Where(b => b.Year == year && b.Month == month)
            .ToListAsync();
    }

    public async Task<Budget?> GetBudgetAsync(TransactionCategory category, int year, int month)
    {
        var db = await GetDatabaseAsync();
        return await db.Table<Budget>()
            .FirstOrDefaultAsync(b => b.Category == category && b.Year == year && b.Month == month);
    }

    public async Task<int> SaveBudgetAsync(Budget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);
        if (budget.Month is < 1 or > 12 || budget.Year is < 1 or > 9999)
            throw new ArgumentOutOfRangeException(nameof(budget), "Budget month or year is invalid.");
        if (budget.MonthlyLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(budget.MonthlyLimit), "Budget limit must be greater than zero.");
        if (!Enum.IsDefined(typeof(TransactionCategory), budget.Category) || budget.Category < TransactionCategory.Food)
            throw new ArgumentException("Budgets can only be assigned to expense categories.", nameof(budget));

        var db = await GetDatabaseAsync();
        budget.UpdatedAt = DateTime.Now;

        if (budget.Id == 0)
        {
            var existing = await db.Table<Budget>()
                .FirstOrDefaultAsync(b => b.Category == budget.Category && b.Year == budget.Year && b.Month == budget.Month);
            if (existing is not null)
            {
                budget.Id = existing.Id;
                budget.CreatedAt = existing.CreatedAt;
            }
            else
            {
                budget.CreatedAt = DateTime.Now;
            }
        }

        return budget.Id == 0
            ? await db.InsertAsync(budget)
            : await db.UpdateAsync(budget);
    }

    public async Task<int> DeleteBudgetAsync(Budget budget)
    {
        var db = await GetDatabaseAsync();
        return await db.DeleteAsync(budget);
    }

    // Summary methods
    public async Task<FinancialSummary> GetMonthlySummaryAsync(int year, int month)
    {
        // Read transactions once and derive all summary values from the same snapshot.
        var transactions = await GetTransactionsByMonthAsync(year, month);
        var incomeTransactions = transactions.Where(t => t.Type == TransactionType.Income).ToList();
        var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
        var income = incomeTransactions.Sum(t => t.Amount);
        var expense = expenseTransactions.Sum(t => t.Amount);
        var expensesByCategory = expenseTransactions
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        var incomeByCategory = incomeTransactions
            .GroupBy(t => t.Category)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        var budgets = await GetBudgetsAsync(year, month);

        var budgetDict = budgets
            .GroupBy(b => b.Category)
            .ToDictionary(group => group.Key,
                group => group.OrderByDescending(b => b.UpdatedAt).First().MonthlyLimit);

        return new FinancialSummary
        {
            Year = year,
            Month = month,
            TotalIncome = income,
            TotalExpense = expense,
            NetBalance = income - expense,
            ExpensesByCategory = expensesByCategory,
            IncomeByCategory = incomeByCategory,
            Budgets = budgetDict
        };
    }
}

public class FinancialSummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBalance { get; set; }
    public Dictionary<TransactionCategory, decimal> ExpensesByCategory { get; set; } = new();
    public Dictionary<TransactionCategory, decimal> IncomeByCategory { get; set; } = new();
    public Dictionary<TransactionCategory, decimal> Budgets { get; set; } = new();

    public string MonthDisplay => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public string FormattedIncome => TotalIncome.ToString("C");
    public string FormattedExpense => TotalExpense.ToString("C");
    public string FormattedBalance => NetBalance.ToString("C");
    public Color BalanceColor => NetBalance >= 0 ? Colors.Green : Colors.Red;
}